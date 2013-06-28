angular.module('app.controllers', ['ngSanitize'])
.controller('HomeCtrl', ['$scope', function ($scope) {
    //SearchService.find($routeParams.id);
}])
.controller('ErrorCtrl',['$scope',function($scope) {
    
}])
.controller('UserCtrl', ['$scope', '$window', '$timeout','authService', function ($scope, $window, $timeout, authService) {
    $scope.service = authService;
    

    $scope.credentials = {
        username: '',
        email: '',
        password: ''//,
        //autologin: false
    };
    
    $scope.profile = function() {
        $scope.service.profile().success(function(response) {
            $scope.credentials = response.data;
        });
    };
    
    $scope.signin = function() {
        $scope.service.login($scope.credentials)
            .success(function (response) {
                console.log(response);
                $window.location="#/";
            }).error(function (response) {
                $scope.error = response;
            });
    };

    $scope.yahooLogin = authService.yahooLogin;
    $scope.googleLogin = authService.googleLogin;

    $scope.logout = function() {
        $scope.service.logout()
            .success(function (response) {
                $window.location = "#/";
            }).error(function (response) {
                $scope.error = response;
            });
    };
    
    $scope.register = function() {
        $scope.service.register($scope.credentials)
            .success(function (response) {
                $window.location = "#/";
            }).error(function(response) {
                $scope.error = response;
            }).validation(function (response) {                
                $scope.validationErrors = response.validationErrors;
            });
    };

    if ($window.location.hash == "#/logout") {
        $timeout($scope.logout, 2000);
    }

}])
.controller('TestCtrl', ['$scope', 'apiService', function ($scope, api) {
    $scope.service = api.service;
    $scope.todos = [];
    $scope.service
        .get("api/todos")
        .success(function (response) {            
            $scope.todos = response.data;
        }).error(function (response) {
            $scope.error = response;
        });
    $scope.deleteTodo = function(todo) {        
        $scope.service.delete("api/todos/"+todo.id)
            .success(function (response) {
                $scope.todos.splice($scope.todos.indexOf(todo), 1);
            }).error(function (response) {
                console.log(response);
            });
    };
}])
   .controller('TestDetailsCtrl',
    ['$scope', '$routeParams', '$window', 'todoService', function ($scope, $routeParams, $window, todoService) {
        $scope.service = todoService.service;
        $scope.todo = {};
        $scope.save = function() {
            $scope.service.put("/api/todos/" + $scope.todo.id, $scope.todo)
                .success(function(response) {
                    console.log("saved...");
                    $window.location = "#/test/" + $scope.todo.id;
                });
        };
        
        $scope.service
                .get("/api/todos/" + $routeParams.id)
                .success(function (response) {                                       
                    $scope.todo = response.data[0];
                })
                .error(function (response) {
                    $scope.error = response;
                });
    } ])
    .controller('NotFoundCtrl', ['$scope', '$routeParams', function ($scope, $routeParams) {

    } ]);
