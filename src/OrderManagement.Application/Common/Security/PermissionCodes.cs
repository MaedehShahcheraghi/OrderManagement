namespace OrderManagement.Application.Common.Security;

public static class PermissionCodes
{
    public const string ReadOrders = "orders.read";
    public const string CreateOrder = "orders.create";
    public const string ConfirmOrder = "orders.confirm";
    public const string ShipOrder = "orders.ship";
    public const string DeliverOrder = "orders.deliver";
    public const string DeleteOrder = "orders.delete";
    public const string BulkCreateOrders = "orders.bulk-create";
    public const string ManagePermissions = "permissions.manage";

    public static IReadOnlyList<string> All { get; } =
    [
        ReadOrders,
        CreateOrder,
        ConfirmOrder,
        ShipOrder,
        DeliverOrder,
        DeleteOrder,
        BulkCreateOrders,
        ManagePermissions
    ];
}
