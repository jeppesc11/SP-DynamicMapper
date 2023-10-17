using Microsoft.SharePoint.Client;
using SP_DynamicMapper.Attributes;
using SP_DynamicMapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication.Models
{
    [SPList(listGuid: "128b4e17-691a-4e28-9934-7bd399ba907a", listTitle: "EmployeeTasks")]
    internal class EmployeeTaskModel
    {
        [SPField(internalName: "ID")]
        public int Id { get; set; }

        [SPField(internalName: "Title")]
        public string? Title { get; set; }

        [SPField(internalName: "EmployeeTask_Done")]
        public bool? Done { get; set; }

        [SPField(internalName: "EmployeeTask_Employeer")]
        public int? EmployeerID { get; set; }

        //[SPField(internalName: "EmployeeTask_Employeer")]
        //public FieldLookupValue? Employeer { get; set; }

        [SPField(internalName: "Employeer_Name", joinFieldInternalName: "EmployeeTask_Employeer", joinListID: "079c9320-2e88-4c48-a7e7-477de96a2c65")]
        public string? Employeer_Name { get; set; }

        [SPField(internalName: "Employeer_Phone", joinFieldInternalName: "EmployeeTask_Employeer", joinListID: "079c9320-2e88-4c48-a7e7-477de96a2c65")]
        public string? Employeer_Phone { get; set; }

    }
}