/***
 * Service: entityManagerFactory 
 *
 * Configures BreezeJS and creates new instance(s) of the 
 * BreezeJS EntityManager for use in a 'datacontext' service
 *
 ***/
(function () {
    'use strict';
    
    var serviceId = 'entityManagerFactory';
    angular.module('app')
           .factory(serviceId, ['breeze', emFactory]);

    function emFactory(breeze) {
        configureBreeze();
        var serviceRoot = window.location.protocol + '//' + window.location.host + '/';
        var serviceName = serviceRoot + 'odata/';
        var factory = {
            newManager: newManager,
            serviceName: serviceName
        };

        return factory;

        function configureBreeze() {           
            // use Web API OData to query and save
        	var odataAdapter = breeze.config.initializeAdapterInstance('dataService', 'webApiOData', true);

        	// Replace getRoutePrefix - https://github.com/Breeze/breeze.js.samples/issues/31
        	odataAdapter.getRoutePrefix = function (dataService) {
				// Get the routePrefix from a Web API OData service name.
				// The routePrefix is presumed to be the pathname within the dataService.serviceName
				// Examples of servicename -> routePrefix:
				//   'http://localhost:55802/odata/' -> '/odata/'
				//   'http://198.154.121.75/service/odata/' -> '/service/odata/'
				if (typeof document === 'object') { // browser
					var parser = document.createElement('a');
					parser.href = dataService.serviceName;
				} else { // node
					parser = url.parse(dataService.serviceName);
				}
				var prefix = parser.pathname;
				if (prefix.substr(-1) !== '/') {
					prefix += '/';
				} // ensure trailing '/'
				return prefix;
			}

            // convert between server-side PascalCase and client-side camelCase
            breeze.NamingConvention.camelCase.setAsDefault();
        }

        function newManager() {
            var mgr = new breeze.EntityManager(serviceName);
            return mgr;
        }


    }
})();