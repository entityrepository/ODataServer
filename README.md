# EntityRepository.ODataServer
Enhances Web API OData functionality, featuring generic OData controllers that can be generated from a domain model, including any Entity Framework DbContext.

This project was previously hosted at https://entityrepository.codeplex.com/.  
https://github.com/entityrepository/ODataServer/ is its new home.

This project was created to meet the following needs:

* To bridge the strengths of WCF Data Services (easy standard odata service from an Entity Framework model)
with Web API OData (flexibility and extensibility)
* To extend Web API OData to better support odata features
* To enable re-using generic odata controllers for multiple entity types - thus reducing the verbosity 
of a Web API odata-based webapp
* To enable easily generating an odata service from any arbitrary domain model, including but not limited 
to an Entity Framework DbContext.
* To fix deficiencies in the Web API OData implementation for better interoperability with browser frameworks that use odata

Despite Web API's clean design, WCF Data Services has the significant advantage that it's very simple to expose an ObjectContext or DbContext as an OData service. This project provides a library for easily exposing an EF code-first DbContext as a Web API OData endpoint. In the simple case, no Controller code needs to be written for each DbSet. For any more complex cases, the default Controllers can be subclassed to provide customized behavior.


