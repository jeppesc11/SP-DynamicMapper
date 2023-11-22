# SP-DynamicMapper

## Summary
This is an extended version of my [POC](https://github.com/jeppesc11/ListItemDynamicMapper). Unlike the POC, this version does not build on [PnP Core SDK](https://pnp.github.io/pnpcore/). The primary emphasis is on minimizing calls to SharePoint and encapsulating a significant amount of boilerplate code.

**Key features:**
- Model mapning
- Expand models easy and fast
- Model change trackings

## Quick Start
Create a model that reflects an existing SharePoint list.

``` csharp
[SPList(listGuid: "128b4e17-691a-4e28-9934-7bd399ba907a", listTitle: "EmployeeTasks")]
internal class EmployeeTaskModel
{
    [SPField(internalName: "ID")]
    public int Id { get; set; }

    [SPField(internalName: "Title")]
    public string? Title { get; set; }

    [SPField(internalName: "EmployeeTask_Done")]
    public bool? Done { get; set; }

    [SPField(internalName: "EmployeeTask_Employeer", SPFieldType.LookupId)]
    public int? EmployeerID { get; set; }

    [SPField(internalName: "Employeer_Name", joinFieldInternalName: "EmployeeTask_Employeer", joinListID: "079c9320-2e88-4c48-a7e7-477de96a2c65")]
    public string? Employeer_Name { get; set; }

    [SPField(internalName: "Employeer_Phone", joinFieldInternalName: "EmployeeTask_Employeer", joinListID: "079c9320-2e88-4c48-a7e7-477de96a2c65")]
    public string? Employeer_Phone { get; set; }
}
```


To enable automatic tracking of changes, you can achieve this by simply adding the following:

``` csharp
[SPList(listGuid: "128b4e17-691a-4e28-9934-7bd399ba907a", listTitle: "EmployeeTasks")]
internal class EmployeeTaskModel : TrackModel<EmployeeTaskModel>
{
```

To retrieve the elements in the SharePoint list, one simply needs to make the following call.
``` csharp
using (PnPContext pnpContext = pnpContextFactory.CreateAsync(new Uri(siteUrl)).GetAwaiter().GetResult())
{
    using (ClientContext context = new AuthenticationManager(pnpContext).GetContext(siteUrl))
    {
        context.Load(context.Web);

        IEnumerable<EmployeeTaskModel> items = context.Web.GetItems<EmployeeTaskModel>();
    }
}
```

To expand the lookup fields:

```csharp
IEnumerable<EmployeeTaskModel> items = context.Web.GetItems<EmployeeTaskModel>(x =>
{
    x.Includes(p => p.Employeer_Name, p => p.Employeer_Phone);
});
```

To filter the items:

```csharp 
IEnumerable<EmployeeTaskModel> items = context.Web.GetItems<EmployeeTaskModel>(x =>
{
    x.Where(p => p.EmployeerID == 6 && p.Title == "N/A");
});
```

Both can be done simultaneously:

```csharp
IEnumerable<EmployeeTaskModel> items = context.Web.GetItems<EmployeeTaskModel>(x =>
{
    x.Includes(p => p.Employeer_Name, p => p.Employeer_Phone);
    x.Where(p => p.EmployeerID == 6 && p.Title == "N/A");
});
```

## Authors

* [**Jeppe Spanggaard**](https://github.com/jeppesc11) - *Research and Development*