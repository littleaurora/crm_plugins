using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace AutoNumberingPlugin
{
    // This plugin is used to generate account number when the new record is created
    public class GetLatestRecord : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {            
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Check whether the target is correct
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                
                if (entity.LogicalName != "account")
                    return;
                
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    // fetch the account number of latest record
                    string fetchxml = @"<fetch mapping='logical'>
                            <entity name = 'account'>
                                <attribute name = 'accountnumber' />
                                <order attribute = 'createdon' descending = 'true' />
                            </entity>
                        </fetch>";
                    EntityCollection ec = service.RetrieveMultiple(new FetchExpression(fetchxml));
                    int accountNumber = Convert.ToInt32(ec.Entities[0].Attributes["accountnumber"]);

                    // add accountnumber to new record
                    entity.Attributes.Add("accountnumber", (accountNumber+1).ToString());
                }
                catch (Exception ex)
                {
                    tracingService.Trace("AutoNumberingPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
