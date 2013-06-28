'use strict';


var app = angular.module('app',
        ['ui', 'ui.bootstrap','angular-servicestack', 'app.filters', 'app.services', 'app.directives', 'app.controllers'])
    .config(['$routeProvider', function ($routeProvider) {
        
        $routeProvider.when('/', { templateUrl: 'partials/home.html', controller: 'HomeCtrl' });
        $routeProvider.when('/register', { templateUrl: 'partials/register.html', controller: 'UserCtrl' });
        $routeProvider.when('/login', { templateUrl: 'partials/login.html', controller: 'UserCtrl' });
        $routeProvider.when('/logout', { templateUrl: 'partials/logout.html', controller: 'UserCtrl' });
        $routeProvider.when('/test', { templateUrl: 'partials/test.html', controller: 'TestCtrl' });
        $routeProvider.when('/test/:id', { templateUrl: 'partials/testdetails.html', controller: 'TestDetailsCtrl' });
        $routeProvider.when('/test/:id/edit', { templateUrl: 'partials/testedit.html', controller: 'TestDetailsCtrl' });
        $routeProvider.when('/error', { templateUrl: 'partials/error.html', controller: 'ErrorCtrl' });
        $routeProvider.when('/404', { templateUrl: 'partials/404.html', controller: 'NotFoundCtrl' });
        $routeProvider.otherwise({ redirectTo: '/404' });
    } ]);
    
    angular.module('angular-servicestack').
        config(function(serviceStackRestConfigProvider) {
            serviceStackRestConfigProvider.setRestConfig ={
            urlPrefix: "/api/",
            maxRetries: 3,
            maxDelayBetweenRetries: 4000,
            unauthorizedFn: function(response, $location) { 
                continuePath = encodeURIComponent($location.path());
                $location.path("/a/signin/#{ continuePath }");
            }
        }
    });