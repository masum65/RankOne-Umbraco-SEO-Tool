﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Script.Serialization;
using RankOne.Models;
using RankOne.Repositories;
using RankOne.Services;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace RankOne.Controllers
{
    [PluginController("RankOne")]
    public class DashboardApiController : UmbracoAuthorizedApiController
    {
        private readonly UmbracoHelper _umbracoHelper;
        private readonly NodeReportRepository _nodeReportRepository;
        private readonly JavaScriptSerializer _javascriptSerializer;
        private readonly AnalyzeService _analyzeService;

        public DashboardApiController()
        {
            _umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
            _nodeReportRepository = new NodeReportRepository();
            _javascriptSerializer = new JavaScriptSerializer();
            _analyzeService = new AnalyzeService();
        }

        [HttpGet]
        public IEnumerable<HiearchyNode> Initialize()
        {
            if (!_nodeReportRepository.DatabaseExists())
            {
                _nodeReportRepository.CreateTable();
            }

            return UpdateAllPages();
        }

        [HttpGet]
        public IEnumerable<HiearchyNode> GetPageHierarchy()
        {
            if (_nodeReportRepository.DatabaseExists())
            {
                var nodeCollection = _umbracoHelper.TypedContentAtRoot();
                var nodeHierarchy = GetHierarchy(nodeCollection);

                GetPageScores(nodeHierarchy);

                return nodeHierarchy;
            }
            return null;
        }

        [HttpGet]
        public IEnumerable<HiearchyNode> UpdateAllPages()
        {
            var nodeCollection = _umbracoHelper.TypedContentAtRoot();
            var nodeHierarchy = GetHierarchy(nodeCollection);

            foreach (var node in nodeHierarchy)
            {
                UpdatePageScore(node);
            }

            return nodeHierarchy;
        }

        private void GetPageScores(IEnumerable<HiearchyNode> nodeHierarchy)
        {
            foreach (var node in nodeHierarchy)
            {
                var nodeReport = _nodeReportRepository.GetById(node.NodeInformation.Id);
                if (nodeReport != null)
                {
                    if (node.NodeInformation.TemplateId == 0)
                    {
                        _nodeReportRepository.Delete(nodeReport);
                    }
                    if (node.NodeInformation.TemplateId > 0 || node.HasChildrenWithTemplate)
                    {
                        node.FocusKeyword = nodeReport.FocusKeyword;
                        try
                        {
                            node.PageScore = _javascriptSerializer.Deserialize<PageScore>(nodeReport.Report);
                        }
                        catch (Exception)
                        {
                            // delete database copy
                            _nodeReportRepository.Delete(nodeReport);
                        }
                    }
                }
                GetPageScores(node.Children);
            }
        }

        private void UpdatePageScore(HiearchyNode node)
        {
            if (node.NodeInformation.TemplateId > 0)
            {
                var umbracoNode = _umbracoHelper.TypedContent(node.NodeInformation.Id);
                var analysis = _analyzeService.CreateAnalysis(umbracoNode);

                node.FocusKeyword = analysis.FocusKeyword;
                node.PageScore = analysis.Score;
            }
            foreach (var childNode in node.Children)
            {
                UpdatePageScore(childNode);
            }
        }

        private List<HiearchyNode> GetHierarchy(IEnumerable<IPublishedContent> nodeCollection)
        {
            var nodeHiearchyCollection = new List<HiearchyNode>();
            foreach (var node in nodeCollection)
            {

                var nodeHierarchy = new HiearchyNode
                {
                    NodeInformation = new NodeInformation
                    {
                        Id = node.Id,
                        Name = node.Name,
                        TemplateId = node.TemplateId
                    },
                    Children = GetHierarchy(node.Children)
                };

                nodeHiearchyCollection.Add(nodeHierarchy);
            }

            return nodeHiearchyCollection.ToList();
        }
    }
}
