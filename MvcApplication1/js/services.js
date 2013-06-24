angular.module('app.services', ['angular-servicestack'])
    .factory('apiService', ['serviceStackRestClient', function (servicestack) {
        return {
            service: servicestack
        };
    }])
    .factory('authService', ['serviceStackRestClient', function(servicestack) {
        var service = {};
        service.isProcessing = false;
        service.profile = {};

        service.register = function (credentials) {
            return servicestack.post("/api/register", credentials);
        };

        service.yahooLogin = function () {
            return servicestack.post("/api/auth/yahooopenid");
        };

        service.googleLogin = function () {
            return servicestack.post("/api/auth/googleopenid");
        };

        service.login = function (credentials) {
            return servicestack.post("/api/auth/credentials", credentials);
        };
        
        service.logout = function () {
            return servicestack.get("/api/auth/logout");
        };

        service.getProfile = function() {
            return servicestack.get("/api/profile");
        };
        
        return service;
    }]);