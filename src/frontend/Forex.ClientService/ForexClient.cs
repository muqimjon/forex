namespace Forex.ClientService;

using Forex.ClientService.Interfaces;

public class ForexClient(
    IApiAuth auth,
    IApiUser users,
    IApiSemiProducts semiProduct,
    IApiManufactory manufactory,
    IApiProductTypes productType,
    IApiProductTypeItems productTypeItems,
    IApiCurrency currency,
    IApiUnitMeasures unitMeasure,
    IApiProducts products,
    IApiSemiProductEntry semiProductEntry,
    IApiProductEntries productEntries,
    IApiSales sales,
    IApiTransactions transactions,
    IApiShops shops,
    IApiProcesses processes,
    IApiProductResidues productResidues)
{
    public IApiAuth Auth { get; } = auth;
    public IApiUser Users { get; } = users;
    public IApiSemiProducts SemiProduct { get; } = semiProduct;
    public IApiManufactory Manufactories { get; } = manufactory;
    public IApiCurrency Currencies { get; } = currency;
    public IApiUnitMeasures UnitMeasures { get; } = unitMeasure;
    public IApiProducts Products { get; } = products;
    public IApiProductTypes ProductTypes { get; } = productType;
    public IApiProductTypeItems ProductTypeItems { get; } = productTypeItems;
    public IApiSemiProductEntry SemiProductEntry { get; } = semiProductEntry;
    public IApiProductEntries ProductEntries { get; } = productEntries;
    public IApiSales Sales { get; } = sales;
    public IApiTransactions Transactions { get; set; } = transactions;
    public IApiShops Shops { get; set; } = shops;
    public IApiProcesses Processes { get; set; } = processes;
    public IApiProductResidues ProductResidues { get; set; } = productResidues;
}
