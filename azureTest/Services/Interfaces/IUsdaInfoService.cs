using azureTest.Models;

namespace azureTest.Services.Interfaces;

public interface IUsdaInfoService
{
    //List<UsdaInfo> GetUsdaInfos();
    List<Datum> GetUsdaDataObjectsNoParams();
    List<Datum> GetUsdaDataObjectsOld(string metric, string commodity, string year);
    List<Datum> GetUsdaDataObjects(string metric, string commodity, string year, string short_desc);
    List<Datum> GetUsdaDataObjectsByState(string metric, string commodity, string year, string short_desc, string stateAlpha);
    Task<List<Datum>> GetUsdaDataObjectsRefactored(string metric, string commodity, string year, string short_desc);
    Task<List<Datum>> GetUsdaDataObjectsRefactoredMultiYear(string metric, string commodity, string year, string short_desc);
    Task<List<Datum>> GetUsdaDataObjectsStates(string metric, string commodity, string year, string short_desc);
    Task<List<Datum>> CompareMetricTo5YearAverage(string metric, string commodity, string year, string short_desc, string week);
}
