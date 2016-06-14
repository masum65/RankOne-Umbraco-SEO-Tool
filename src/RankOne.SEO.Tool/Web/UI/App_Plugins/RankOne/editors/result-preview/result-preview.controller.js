﻿(function () {

    // Controller
    function rankOne($scope, $http, editorState, webresultService, localizationService) {

        $scope.load = function () {
            $scope.loading = true;

            if (!editorState.current.published) {
                $scope.error = localizationService.localize("error_not_published");
                $scope.loading = false;
            } else {
                var url = '/umbraco/backoffice/api/RankOneApi/GetPageInformation?id={id}';
                webresultService.GetResult(editorState.current, url)
                    .then(function(response) {

                            $scope.result = response;
                            $scope.loading = false;

                        },
                        function(message) {
                            $scope.error = message;
                            $scope.loading = false;
                        });
            }
        }

        $scope.$on("formSubmitted", function () {
            $scope.load();
        });

        $scope.load();
    };

    // Register the controller
    angular.module("umbraco").controller('rankOneResultPreview', rankOne);

})();