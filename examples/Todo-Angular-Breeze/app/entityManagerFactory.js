/***
 * Service: entityManagerFactory 
 *
 * Configures BreezeJS and creates new instance(s) of the 
 * BreezeJS EntityManager for use in a 'datacontext' service
 *
 ***/
(function () {
    'use strict';
    
    angular.module('app')
           .factory('entityManagerFactory', ['breeze', emFactory]);

    function emFactory(breeze) {
        var serviceRoot = window.location.protocol + '//' + window.location.host + '/';
        var serviceUrl = serviceRoot + 'odata/';

        function configureBreeze() {           
            // use Web API OData to query and save
        	var odataAdapter = breeze.config.initializeAdapterInstance('dataService', 'webApiOData', true);

        	// Replace getRoutePrefix - https://github.com/Breeze/breeze.js.samples/issues/31
        	odataAdapter.getRoutePrefix = function (dataService) {
        		return "";
        	}

            // convert between server-side PascalCase and client-side camelCase
        	breeze.NamingConvention.camelCase.setAsDefault();
        }

        function newManager() {
        	var mgr = new breeze.EntityManager(serviceUrl);
            return mgr;
        }

        var factory = {
        	newManager: newManager,
        	serviceName: serviceUrl
        };

        configureBreeze();
        return factory;
    }
})();