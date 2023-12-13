using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF_SeperateCaseTypeField
{
    public class SeperateCaseType : CodeActivity
    {
        protected override void Execute(CodeActivityContext executioncontext)
        {
        
            #region  ------ WF Defualt Code------
            //Creating Tracing Service
            ITracingService tracingserivce = executioncontext.GetExtension<ITracingService>();
            //Create Context
            IWorkflowContext context = executioncontext.GetExtension<IWorkflowContext>();
            //obtain the orgainzation service reference 
            IOrganizationServiceFactory serviceFactory = executioncontext.GetExtension<IOrganizationServiceFactory>();
            //create object from organizationservice
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            OrganizationServiceContext entitycontext = new OrganizationServiceContext(service);
            #endregion
            if (context.Depth > 1)
            {
                return;
            }
            tracingserivce.Trace("Start of Custom Work flow");

            Entity currentEntity = (Entity)context.InputParameters["Target"];
            if(currentEntity.LogicalName == "incident")
            {
                string caseTypeMultiSelect = currentEntity.GetAttributeValue<string>("odh_subcasetypemultiselect");
                tracingserivce.Trace($"Value of case type Multi select is {caseTypeMultiSelect}");
                if (String.IsNullOrEmpty(caseTypeMultiSelect))
                    return;
                string CaseTypeSelected = SeperateNames(caseTypeMultiSelect);
                tracingserivce.Trace($"Value of case type after seperated is {CaseTypeSelected}");
                currentEntity.Attributes["odh_subcasetypecomposide"] = CaseTypeSelected;
                service.Update(currentEntity);
                tracingserivce.Trace($"after update");

            }


        }
        protected  string SeperateNames(string CaseTypesSelected)
        {
            List<string> casetypeObjects = CaseTypesSelected.Split(',').ToList().Where(s => s.StartsWith("\"_name\":\"")).ToList();
            List<string> SubCasesNames = new List<string>();
            foreach (string caseType in casetypeObjects)
            {
                string casename = caseType.Substring(9).Split('"').ToList().FirstOrDefault(s => s != "}");
                if (!string.IsNullOrEmpty(casename))
                {
                    SubCasesNames.Add(casename);
                }
            }

       
            return string.Join(",", SubCasesNames);
        }
    }
}
