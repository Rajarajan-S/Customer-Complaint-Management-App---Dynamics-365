using System;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugin_Dev.Plugins
{
    public class AutonumberForComplaint : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    if (context.MessageName.ToLower() != "create")
                    {
                        return;
                    }

                    Entity targetEntity = context.InputParameters["Target"] as Entity;
                    Entity updateAutoNumber = new Entity("rr_complaintautonumberconfiguration");
                    StringBuilder autoNumber = new StringBuilder();
                    string prefix = string.Empty, separator = string.Empty, currentnumber = string.Empty, year, month, date;
                    DateTime today = DateTime.Now;

                    date = today.Day.ToString("00");
                    month = today.Month.ToString("00");
                    year = today.Year.ToString();

                    QueryExpression AutonumberCombineQuery = new QueryExpression("rr_complaintautonumberconfiguration")
                    {
                        ColumnSet = new ColumnSet("rr_prefix", "rr_separator", "rr_currentnumber", "rr_name")
                    };

                    EntityCollection ECAutonumberCombineQuery = service.RetrieveMultiple(AutonumberCombineQuery);

                    if (ECAutonumberCombineQuery.Entities.Count == 0)
                    {
                        trace.Trace("No autonumber configuration records found.");
                        return;
                    }
                    foreach (Entity entity in ECAutonumberCombineQuery.Entities)
                    {
                        if (entity.Contains("rr_name") && entity.GetAttributeValue<string>("rr_name").ToLower() == "complaintautonumber")
                        {
                            prefix = entity.GetAttributeValue<string>("rr_prefix");
                            separator = entity.GetAttributeValue<string>("rr_separator");
                            currentnumber = entity.GetAttributeValue<string>("rr_currentnumber");

                            if (int.TryParse(currentnumber, out int temp))
                            {
                                temp++;

                                updateAutoNumber.Id = entity.Id;
                                updateAutoNumber["rr_currentnumber"] = temp.ToString(); 
                                service.Update(updateAutoNumber); 
                                autoNumber.Append(prefix + separator + year + month + date + separator + temp.ToString());
                                break;
                            }
                            else
                            {
                                trace.Trace("Invalid current number value.");
                            }
                        }
                    }

                    targetEntity["rr_complaintnumber"] = autoNumber.ToString();
                    service.Update(targetEntity);
                }
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException($"An error occurred in the AutonumberForComplaint plugin: {e.Message}", e);
            }
        }
    }
}
