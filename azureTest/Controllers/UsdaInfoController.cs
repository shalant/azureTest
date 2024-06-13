using Microsoft.AspNetCore.Mvc;
using azureTest.Config;
using Microsoft.Extensions.Options;
using azureTest.Services.Interfaces;

namespace azureTest.Controllers;

[Route("api/[controller]")]
//[Route("/")]
[ApiController]
public class UsdaInfoController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private Usda _usdaConfig;
    private readonly IUsdaInfoService _usdaInfoService;

    public UsdaInfoController(IOptions<Usda> opts, IUsdaInfoService usdaInfoService)
    {
        _httpClient = new HttpClient();
        _usdaConfig = opts.Value;
        _usdaInfoService = usdaInfoService;
    }

    [HttpGet]
    [Route("")]
    public IActionResult GetUsdaDataAsObjects(string Metric, string Commodity, string Year, string short_desc)
    {
        var usdaDataObjects = _usdaInfoService.GetUsdaDataObjects(Metric, Commodity, Year, short_desc);
        return Ok(usdaDataObjects);
    }
    
    [HttpGet]
    [Route("/api/GetUsdaDataRefactored")]
    public async Task<IActionResult> GetUsdaDataAsObjectsRefactored(string Metric, string Commodity, string Year, string short_desc)
    {
        var usdaDataObjects = await _usdaInfoService.GetUsdaDataObjectsRefactored(Metric, Commodity, Year, short_desc);
        return Ok(usdaDataObjects);
    }
    
    [HttpGet]
    [Route("/api/GetMetricVsAverage")]
    public async Task<IActionResult> GetMetricVsAverage(string Metric, string Commodity, string Year, string short_desc, string week)
    {
        var usdaDataObjects = await _usdaInfoService.CompareMetricTo5YearAverage(Metric, Commodity, Year, short_desc, week);
        return Ok(usdaDataObjects);
    }


    
    [HttpGet]
    [Route("/api/GetUsdaDataStates")]
    public async Task<IActionResult> GetUsdaDataAsObjectsStates(string Metric, string Commodity, string Year, string short_desc)
    {
        var usdaDataObjects = await _usdaInfoService.GetUsdaDataObjectsStates(Metric, Commodity, Year, short_desc);
        return Ok(usdaDataObjects);
    }
    
    [HttpGet]
    [Route("/api/GetUsdaDataRefactoredMultiYear")]
    public async Task<IActionResult> GetUsdaDataObjectsRefactoredMultiYear(string Metric, string Commodity, string Year, string short_desc)
    {
        var usdaDataObjects = await _usdaInfoService.GetUsdaDataObjectsRefactoredMultiYear(Metric, Commodity, Year, short_desc);
        return Ok(usdaDataObjects);
    }
    
    [HttpGet]
    [Route("/api/GetByState")]
    public IActionResult GetUsdaDataAsObjectsByState(string Metric, string Commodity, string Year, string short_desc, string stateAlpha)
    {
        var usdaDataObjects = _usdaInfoService.GetUsdaDataObjectsByState(Metric, Commodity, Year, short_desc, stateAlpha);
        return Ok(usdaDataObjects);
    }

    [HttpGet]
    [Route("/api/UsdaInfoOld")]

    public IActionResult GetUsdaDataAsObjectsOld(string Metric, string Commodity, string Year)
    {
        var usdaDataObjects = _usdaInfoService.GetUsdaDataObjectsOld(Metric, Commodity, Year);
        return Ok(usdaDataObjects);
    }
    

    [HttpGet]
    [Route("/getnoparams")]
    public IActionResult GetUsdaDataAsObjectsNoParams()
    {
        var usdaDataObjects = _usdaInfoService.GetUsdaDataObjectsNoParams();
        return Ok(usdaDataObjects);
    }





    // GET: /api/mydata
    //[HttpGet]
    //public async Task<IActionResult> GetUsdaData()
    //{
    //    var myData = await myDataRepository.GetAllMyDataAsync();

    //    // Map domain model to DTO
    //    var response = new List<MyDataDto>();
    //    foreach (var item in  myData)
    //    {
    //        response.Add(new MyDataDto
    //        {
    //            Id = item.Id,
    //            UserFirstName = item.UserFirstName,
    //            UserLastName = item.UserLastName,
    //            CreatedDate = item.CreatedDate,
    //            LastModifiedDate = item.LastModifiedDate,
    //            prodn_practice_desc = item.prodn_practice_desc,
    //            domain_desc = item.domain_desc,
    //            county_name = item.county_name,
    //            freq_desc = item.freq_desc,
    //            begin_code = item.begin_code,
    //            watershed_code = item.watershed_code,
    //            end_code = item.end_code,
    //            state_alpha = item.state_alpha,
    //            agg_level_desc = item.agg_level_desc,
    //            CV = item.CV,
    //            state_ansi = item.state_ansi,
    //            util_practice_desc = item.util_practice_desc,
    //            region_desc = item.region_desc,
    //            state_fips_code = item.state_fips_code,
    //            county_code = item.county_code,
    //            week_ending = item.week_ending,
    //            year = item.year,
    //            watershed_desc = item.watershed_desc,
    //            unit_desc = item.unit_desc,
    //            country_name = item.country_name,
    //            domaincat_desc = item.domaincat_desc,
    //            location_desc = item.location_desc,
    //            zip_5 = item.zip_5,
    //            group_desc = item.group_desc,
    //            load_time = item.load_time,
    //            Value = item.Value,
    //            asd_desc = item.asd_desc,
    //            county_ansi = item.county_ansi,
    //            asd_code = item.asd_code,
    //            commodity_desc = item.commodity_desc,
    //            statisticcat_desc = item.statisticcat_desc,
    //            congr_district_code = item.congr_district_code,
    //            state_name = item.state_name,
    //            reference_period_desc = item.reference_period_desc,
    //            source_desc = item.source_desc,
    //            class_desc = item.class_desc,
    //            sector_desc = item.sector_desc,
    //            country_code = item.country_code,
    //            short_desc = item.short_desc
    //        });
    //    }

    //    return Ok(response);
    //}

    // GET: /api/mydata
    //[HttpGet]
    ////public async Task<IActionResult> GetUsdaData()
    //public async Task<IActionResult> GetUsdaData(string Metric, string Commodity, string Year)
    //{

    //    //var apiUrl = $"{baseUrl}{_usdaConfig.ApiKey}&statisticcat_desc=AREA PLANTED&unit_desc=ACRES&year__GE=2020";
    //    var apiUrl = $"{baseUrl}{_usdaConfig.ApiKey}&statisticcat_desc={Metric}&unit_desc=ACRES&year__GE={Year}&commodity_desc={Commodity}";
    //    var dataObject = new UsdaInfo();
    //    //var dataSet = dataObject?.data.ToList();

    //    HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

    //    if (response.IsSuccessStatusCode)
    //    {
    //        string data = await response.Content.ReadAsStringAsync();
    //        //string data = await response.Content.ReadFromJsonAsync<string>();
    //        var json = response.Content.ReadAsStringAsync().Result;

    //        //dataSet = JsonSerializer.Deserialize<UsdaInfo>(json);

    //        return Ok(data);
    //    }
    //    else
    //    {
    //        return StatusCode((int)response.StatusCode);
    //    }

    //    //var usdaData = _usdaInfoService.GetUsdaInfos();
    //    //return Ok(usdaData);
    //}
}
