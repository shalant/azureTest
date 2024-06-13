using Microsoft.Extensions.Options;
using System.Text.Json;
using azureTest.Config;
using azureTest.Models;
using azureTest.Services.Interfaces;

namespace azureTest.Services;

public class UsdaInfoService : IUsdaInfoService
{
    private Usda _usdaConfig;
    private readonly IHttpClientFactory _httpFactory;

    private string url = "";
    private string url1 = "";
    private string url2 = "";

    public UsdaInfoService(IOptions<Usda> opts,
        IHttpClientFactory httpFactory)
    {
        _usdaConfig = opts.Value;
        _httpFactory = httpFactory;
    }

    private string baseUrl = $"https://quickstats.nass.usda.gov/api/api_GET/?key=";

    public async Task<List<Datum>> GetUsdaDataObjectsRefactored(string metric, string commodity, string year, string short_desc)
    {
        url = BuildUsdaUrl(metric, commodity, year, short_desc);

        var dataObjects = new List<Datum>();
        var sortedDataObjects = new List<Datum>();

        var client = _httpFactory.CreateClient();
        var response = await client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var usdaResponse = JsonSerializer.Deserialize<UsdaInfo>(json);
        foreach (var item in usdaResponse.data)
        {
            if (item.agg_level_desc == "NATIONAL")
            {
                dataObjects.Add(new Datum
                {
                    prodn_practice_desc = item.prodn_practice_desc,
                    domain_desc = item.domain_desc,
                    county_name = item.county_name,
                    freq_desc = item.freq_desc,
                    begin_code = item.begin_code,
                    watershed_code = item.watershed_code,
                    end_code = item.end_code,
                    state_alpha = item.state_alpha,
                    agg_level_desc = item.agg_level_desc,
                    CV = item.CV,
                    state_ansi = item.state_ansi,
                    util_practice_desc = item.util_practice_desc,
                    region_desc = item.region_desc,
                    state_fips_code = item.state_fips_code,
                    county_code = item.county_code,
                    week_ending = item.week_ending,
                    year = item.year,
                    watershed_desc = item.watershed_desc,
                    unit_desc = item.unit_desc,
                    country_name = item.country_name,
                    domaincat_desc = item.domaincat_desc,
                    location_desc = item.location_desc,
                    zip_5 = item.zip_5,
                    group_desc = item.group_desc,
                    load_time = item.load_time,
                    Value = item.Value,
                    asd_desc = item.asd_desc,
                    county_ansi = item.county_ansi,
                    asd_code = item.asd_code,
                    commodity_desc = item.commodity_desc,
                    statisticcat_desc = item.statisticcat_desc,
                    congr_district_code = item.congr_district_code,
                    state_name = item.state_name,
                    reference_period_desc = item.reference_period_desc,
                    source_desc = item.source_desc,
                    class_desc = item.class_desc,
                    sector_desc = item.sector_desc,
                    country_code = item.country_code,
                    short_desc = item.short_desc
                }
                );
            }
        }

        // TODO: add filtering (maybe a switch case grabbed from front end...)

        sortedDataObjects = dataObjects.AsEnumerable()
            .Where(x => x.domain_desc == "TOTAL" || x.location_desc == "US TOTAL")
            .Where(x => x.source_desc != "CENSUS")
            .OrderByDescending(x => x.year)
            // TODO: consider filtering by reference_period_desc
            .ThenBy(x => x.load_time)
            .ToList();
        return sortedDataObjects;
    }

    public async Task<List<Datum>> CompareMetricTo5YearAverage(string metric, string commodity, string year, string short_desc, string week)
    {
        // 1st calculation: Progress from selected year
        // 2nd calculation: 5-year progress from selected year
        // TODO: account for 10 scenarios, should i use the trim method?
        if(metric.Substring(0, 8) == "PROGRESS")
        //if (metric == "PROGRESS, 5 YEAR AVG, MEASURED IN PCT PLANTED")
        {
            //THIS WORKS
            //short_desc = $"{commodity} - PROGRESS, MEASURED IN PCT PLANTED";
            short_desc = $"{commodity} - {metric.Remove(10,12)}";
            url1 = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";

            //short_desc = $"{commodity} - PROGRESS, 5 YEAR AVG, MEASURED IN PCT PLANTED";
            short_desc = $"{commodity} - {metric}";
            url2 = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";
        }
        // TODO: add GOOD plus EXCELLENT
        else if (metric == "CONDITION, 5 YEAR AVG, MEASURED IN PCT EXCELLENT")
        {
            short_desc = $"{commodity} - CONDITION, MEASURED IN PCT EXCELLENT";
            url1 = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";

            short_desc = $"{commodity} - CONDITION, 5 YEAR AVG, MEASURED IN PCT EXCELLENT";
            url2 = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";
        }
        var dataObjects1 = new List<Datum>();
        var dataObjects2 = new List<Datum>();
        var sortedDataObjects1 = new List<Datum>();
        var sortedDataObjects2 = new List<Datum>();
        var ksDataObject1 = new List<Datum>();
        var ksDataObject2 = new List<Datum>();
        var summedDataObjects = new List<Datum>();
        //int sum = 0;
        //string originalValue = "";
        //string averageValue = "";

        var client = _httpFactory.CreateClient();
        var response1 = await client.GetAsync(url1);
        var response2 = await client.GetAsync(url2);

        var json1 = await response1.Content.ReadAsStringAsync();
        var json2 = await response2.Content.ReadAsStringAsync();
        var usdaResponse1 = JsonSerializer.Deserialize<UsdaInfo>(json1);
        var usdaResponse2 = JsonSerializer.Deserialize<UsdaInfo>(json2);
        //foreach (var item in usdaResponse1.data.Where(x => x.reference_period_desc == week))
        foreach (var item in usdaResponse1.data)
        {
            if (item.agg_level_desc != "NATIONAL")
            {
                dataObjects1.Add(new Datum
                {
                    prodn_practice_desc = item.prodn_practice_desc,
                    domain_desc = item.domain_desc,
                    county_name = item.county_name,
                    freq_desc = item.freq_desc,
                    begin_code = item.begin_code,
                    watershed_code = item.watershed_code,
                    end_code = item.end_code,
                    state_alpha = item.state_alpha,
                    agg_level_desc = item.agg_level_desc,
                    CV = item.CV,
                    state_ansi = item.state_ansi,
                    util_practice_desc = item.util_practice_desc,
                    region_desc = item.region_desc,
                    state_fips_code = item.state_fips_code,
                    county_code = item.county_code,
                    week_ending = item.week_ending,
                    year = item.year,
                    watershed_desc = item.watershed_desc,
                    unit_desc = item.unit_desc,
                    country_name = item.country_name,
                    domaincat_desc = item.domaincat_desc,
                    location_desc = item.location_desc,
                    zip_5 = item.zip_5,
                    group_desc = item.group_desc,
                    load_time = item.load_time,
                    Value = item.Value,
                    asd_desc = item.asd_desc,
                    county_ansi = item.county_ansi,
                    asd_code = item.asd_code,
                    commodity_desc = item.commodity_desc,
                    statisticcat_desc = item.statisticcat_desc,
                    congr_district_code = item.congr_district_code,
                    state_name = item.state_name,
                    reference_period_desc = item.reference_period_desc,
                    source_desc = item.source_desc,
                    class_desc = item.class_desc,
                    sector_desc = item.sector_desc,
                    country_code = item.country_code,
                    short_desc = item.short_desc
                }
                );
            }
        }
        //foreach (var item in usdaResponse2.data.Where(x => x.reference_period_desc == week))
        foreach (var item in usdaResponse2.data)
        {
            if (item.agg_level_desc != "NATIONAL")
            {
                dataObjects2.Add(new Datum
                {
                    prodn_practice_desc = item.prodn_practice_desc,
                    domain_desc = item.domain_desc,
                    county_name = item.county_name,
                    freq_desc = item.freq_desc,
                    begin_code = item.begin_code,
                    watershed_code = item.watershed_code,
                    end_code = item.end_code,
                    state_alpha = item.state_alpha,
                    agg_level_desc = item.agg_level_desc,
                    CV = item.CV,
                    state_ansi = item.state_ansi,
                    util_practice_desc = item.util_practice_desc,
                    region_desc = item.region_desc,
                    state_fips_code = item.state_fips_code,
                    county_code = item.county_code,
                    week_ending = item.week_ending,
                    year = item.year,
                    watershed_desc = item.watershed_desc,
                    unit_desc = item.unit_desc,
                    country_name = item.country_name,
                    domaincat_desc = item.domaincat_desc,
                    location_desc = item.location_desc,
                    zip_5 = item.zip_5,
                    group_desc = item.group_desc,
                    load_time = item.load_time,
                    Value = item.Value,
                    asd_desc = item.asd_desc,
                    county_ansi = item.county_ansi,
                    asd_code = item.asd_code,
                    commodity_desc = item.commodity_desc,
                    statisticcat_desc = item.statisticcat_desc,
                    congr_district_code = item.congr_district_code,
                    state_name = item.state_name,
                    reference_period_desc = item.reference_period_desc,
                    source_desc = item.source_desc,
                    class_desc = item.class_desc,
                    sector_desc = item.sector_desc,
                    country_code = item.country_code,
                    short_desc = item.short_desc
                }
                );
            }
        }

        // TODO: add filtering (maybe a switch case grabbed from front end...)

        sortedDataObjects1 = dataObjects1.AsEnumerable()
            .Where(x => x.domain_desc == "TOTAL" || x.location_desc != "US TOTAL")
            .Where(x => x.source_desc != "CENSUS" && x.agg_level_desc != "NATIONAL")
            .Where(x => x.reference_period_desc == week)
            .OrderByDescending(x => x.year)
            // TODO: consider filtering by reference_period_desc
            .ThenBy(x => x.load_time)
            .ToList();
        sortedDataObjects2 = dataObjects2.AsEnumerable()
            //.Where(x => x.domain_desc == "TOTAL" || x.location_desc == "US TOTAL")
            //.Where(x => x.source_desc != "CENSUS")
            .Where(x => x.domain_desc == "TOTAL" || x.location_desc != "US TOTAL")
            .Where(x => x.source_desc != "CENSUS" && x.agg_level_desc != "NATIONAL")
            .Where(x => x.reference_period_desc == week)
            .OrderByDescending(x => x.year)
            // TODO: consider filtering by reference_period_desc
            .ThenBy(x => x.load_time)
            .ToList();

        // Here I want to make an array of 50 datums where the values are subtracted
        // loop through each state and push to summedDataObjects array?

        //ksDataObject1 = dataObjects1.Where(x => x.state_alpha == "KS" && x.reference_period_desc ==  week).ToList();
        //ksDataObject2 = dataObjects2.Where(x => x.state_alpha == "KS" && x.reference_period_desc == week).ToList();
        //sum = Int32.Parse(ksDataObject1[0].Value) - Int32.Parse(ksDataObject2[0].Value);

        //foreach (var item in usdaResponse1.data)
        //for (int i = 0; int i < usdaResponse1.data.Length; int i++)
        //for (int i = 0; i < usdaResponse2.data.Length - 1; i++)
        for (int i = 0; i < sortedDataObjects2.Count; i++)
            //for (int i = 0; i < dataObjects1.Count; i++)
        {
            //I think this line is dundant
            //if (sortedDataObjects1[i].agg_level_desc != "NATIONAL" && sortedDataObjects1[i].reference_period_desc == week)
                //if (sortedDataObjects1[i].state_alpha == sortedDataObjects2[i].state_alpha)
                for (int j = 0; j < sortedDataObjects1.Count; j++)
            {
                if (sortedDataObjects2[i].state_alpha == sortedDataObjects1[j].state_alpha)
                summedDataObjects.Add(new Datum
                {
                    prodn_practice_desc = sortedDataObjects1[i].prodn_practice_desc,
                    domain_desc = sortedDataObjects1[i].domain_desc,
                    county_name = sortedDataObjects1[i].county_name,
                    freq_desc = sortedDataObjects1[i].freq_desc,
                    begin_code = sortedDataObjects1[i].begin_code,
                    watershed_code = sortedDataObjects1[i].watershed_code,
                    end_code = sortedDataObjects1[i].end_code,
                    state_alpha = sortedDataObjects1[i].state_alpha,
                    agg_level_desc = sortedDataObjects1[i].agg_level_desc,
                    CV = sortedDataObjects1[i].CV,
                    state_ansi = sortedDataObjects1[i].state_ansi,
                    util_practice_desc = sortedDataObjects1[i].util_practice_desc,
                    region_desc = sortedDataObjects1[i].region_desc,
                    state_fips_code = sortedDataObjects1[i].state_fips_code,
                    county_code = sortedDataObjects1[i].county_code,
                    week_ending = sortedDataObjects1[i].week_ending,
                    year = sortedDataObjects1[i].year,
                    watershed_desc = sortedDataObjects1[i].watershed_desc,
                    unit_desc = sortedDataObjects1[i].unit_desc,
                    country_name = sortedDataObjects1[i].country_name,
                    domaincat_desc = sortedDataObjects1[i].domaincat_desc,
                    location_desc = sortedDataObjects1[i].location_desc,
                    zip_5 = sortedDataObjects1[i].zip_5,
                    group_desc = sortedDataObjects1[i].group_desc,
                    load_time = sortedDataObjects1[i].load_time,
                    //Value = (Int32.Parse(dataObjects1.Where(x => x.state_alpha == "AR").FirstOrDefault().Value) - (Int32.Parse(dataObjects2.Where(x => x.state_alpha == "AR").FirstOrDefault().Value))).ToString(),
                    
                    //BUG: when SDO2 doesn't have a state, it throws an error
                    //Do i need a second loop?
                    //Value = (Int32.Parse(sortedDataObjects1.Where(x => x.state_alpha == sortedDataObjects2[i].state_alpha).FirstOrDefault().Value) - (Int32.Parse(sortedDataObjects2.Where(x => x.state_alpha == sortedDataObjects1[j].state_alpha).FirstOrDefault().Value))).ToString(),
                    Value = (Int32.Parse(sortedDataObjects1[j].Value) - Int32.Parse(sortedDataObjects2[i].Value)).ToString(),
                    asd_desc = sortedDataObjects1[i].asd_desc,
                    county_ansi = sortedDataObjects1[i].county_ansi,
                    asd_code = sortedDataObjects1[i].asd_code,
                    commodity_desc = sortedDataObjects1[i].commodity_desc,
                    statisticcat_desc = sortedDataObjects1[i].statisticcat_desc,
                    congr_district_code = sortedDataObjects1[i].congr_district_code,
                    state_name = sortedDataObjects1[i].state_name,
                    reference_period_desc = sortedDataObjects1[i].reference_period_desc,
                    source_desc = sortedDataObjects1[i].source_desc,
                    class_desc = sortedDataObjects1[i].class_desc,
                    sector_desc = sortedDataObjects1[i].sector_desc,
                    country_code = sortedDataObjects1[i].country_code,
                    //short_desc = sortedDataObjects1[i].short_desc
                    short_desc = $"{year} minus five year average"
                }
                );
            }
        }
        //return Int32.Parse(summedDataObjects[1].Value);
        return summedDataObjects;
    }
    
    public async Task<List<Datum>> CompareTo5YearAverage(string metric, string commodity, string year, string short_desc, string week)
    {
        // 1st calculation: Progress from selected year
        // 2nd calculation: 5-year progress from selected year
        short_desc = $"{commodity} - PROGRESS, MEASURED IN PCT EMERGED";
        url1 = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";

        short_desc = $"{commodity} - PROGRESS, 5 YEAR AVG, MEASURED IN PCT EMERGED";
        url2 = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";

        var dataObjects1 = new List<Datum>();
        var dataObjects2 = new List<Datum>();
        var sortedDataObjects1 = new List<Datum>();
        var sortedDataObjects2 = new List<Datum>();
        var ksDataObject1 = new List<Datum>();
        var ksDataObject2 = new List<Datum>();
        var summedDataObjects = new List<Datum>();
        //int sum = 0;
        //string originalValue = "";
        //string averageValue = "";

        var client = _httpFactory.CreateClient();
        var response1 = await client.GetAsync(url1);
        var response2 = await client.GetAsync(url2);

        var json1 = await response1.Content.ReadAsStringAsync();
        var json2 = await response2.Content.ReadAsStringAsync();
        var usdaResponse1 = JsonSerializer.Deserialize<UsdaInfo>(json1);
        var usdaResponse2 = JsonSerializer.Deserialize<UsdaInfo>(json2);
        //foreach (var item in usdaResponse1.data.Where(x => x.reference_period_desc == week))
        foreach (var item in usdaResponse1.data)
        {
            if (item.agg_level_desc != "NATIONAL")
            {
                dataObjects1.Add(new Datum
                {
                    prodn_practice_desc = item.prodn_practice_desc,
                    domain_desc = item.domain_desc,
                    county_name = item.county_name,
                    freq_desc = item.freq_desc,
                    begin_code = item.begin_code,
                    watershed_code = item.watershed_code,
                    end_code = item.end_code,
                    state_alpha = item.state_alpha,
                    agg_level_desc = item.agg_level_desc,
                    CV = item.CV,
                    state_ansi = item.state_ansi,
                    util_practice_desc = item.util_practice_desc,
                    region_desc = item.region_desc,
                    state_fips_code = item.state_fips_code,
                    county_code = item.county_code,
                    week_ending = item.week_ending,
                    year = item.year,
                    watershed_desc = item.watershed_desc,
                    unit_desc = item.unit_desc,
                    country_name = item.country_name,
                    domaincat_desc = item.domaincat_desc,
                    location_desc = item.location_desc,
                    zip_5 = item.zip_5,
                    group_desc = item.group_desc,
                    load_time = item.load_time,
                    Value = item.Value,
                    asd_desc = item.asd_desc,
                    county_ansi = item.county_ansi,
                    asd_code = item.asd_code,
                    commodity_desc = item.commodity_desc,
                    statisticcat_desc = item.statisticcat_desc,
                    congr_district_code = item.congr_district_code,
                    state_name = item.state_name,
                    reference_period_desc = item.reference_period_desc,
                    source_desc = item.source_desc,
                    class_desc = item.class_desc,
                    sector_desc = item.sector_desc,
                    country_code = item.country_code,
                    short_desc = item.short_desc
                }
                );
            }
        }
        //foreach (var item in usdaResponse2.data.Where(x => x.reference_period_desc == week))
        foreach (var item in usdaResponse2.data)
        {
            if (item.agg_level_desc != "NATIONAL")
            {
                dataObjects2.Add(new Datum
                {
                    prodn_practice_desc = item.prodn_practice_desc,
                    domain_desc = item.domain_desc,
                    county_name = item.county_name,
                    freq_desc = item.freq_desc,
                    begin_code = item.begin_code,
                    watershed_code = item.watershed_code,
                    end_code = item.end_code,
                    state_alpha = item.state_alpha,
                    agg_level_desc = item.agg_level_desc,
                    CV = item.CV,
                    state_ansi = item.state_ansi,
                    util_practice_desc = item.util_practice_desc,
                    region_desc = item.region_desc,
                    state_fips_code = item.state_fips_code,
                    county_code = item.county_code,
                    week_ending = item.week_ending,
                    year = item.year,
                    watershed_desc = item.watershed_desc,
                    unit_desc = item.unit_desc,
                    country_name = item.country_name,
                    domaincat_desc = item.domaincat_desc,
                    location_desc = item.location_desc,
                    zip_5 = item.zip_5,
                    group_desc = item.group_desc,
                    load_time = item.load_time,
                    Value = item.Value,
                    asd_desc = item.asd_desc,
                    county_ansi = item.county_ansi,
                    asd_code = item.asd_code,
                    commodity_desc = item.commodity_desc,
                    statisticcat_desc = item.statisticcat_desc,
                    congr_district_code = item.congr_district_code,
                    state_name = item.state_name,
                    reference_period_desc = item.reference_period_desc,
                    source_desc = item.source_desc,
                    class_desc = item.class_desc,
                    sector_desc = item.sector_desc,
                    country_code = item.country_code,
                    short_desc = item.short_desc
                }
                );
            }
        }

        // TODO: add filtering (maybe a switch case grabbed from front end...)

        sortedDataObjects1 = dataObjects1.AsEnumerable()
            .Where(x => x.domain_desc == "TOTAL" || x.location_desc != "US TOTAL")
            .Where(x => x.source_desc != "CENSUS" && x.agg_level_desc != "NATIONAL")
            .Where(x => x.reference_period_desc == week)
            .OrderByDescending(x => x.year)
            // TODO: consider filtering by reference_period_desc
            .ThenBy(x => x.load_time)
            .ToList();
        sortedDataObjects2 = dataObjects2.AsEnumerable()
            //.Where(x => x.domain_desc == "TOTAL" || x.location_desc == "US TOTAL")
            //.Where(x => x.source_desc != "CENSUS")
            .Where(x => x.domain_desc == "TOTAL" || x.location_desc != "US TOTAL")
            .Where(x => x.source_desc != "CENSUS" && x.agg_level_desc != "NATIONAL")
            .Where(x => x.reference_period_desc == week)
            .OrderByDescending(x => x.year)
            // TODO: consider filtering by reference_period_desc
            .ThenBy(x => x.load_time)
            .ToList();

        // Here I want to make an array of 50 datums where the values are subtracted
        // loop through each state and push to summedDataObjects array?

        //ksDataObject1 = dataObjects1.Where(x => x.state_alpha == "KS" && x.reference_period_desc ==  week).ToList();
        //ksDataObject2 = dataObjects2.Where(x => x.state_alpha == "KS" && x.reference_period_desc == week).ToList();
        //sum = Int32.Parse(ksDataObject1[0].Value) - Int32.Parse(ksDataObject2[0].Value);

        //foreach (var item in usdaResponse1.data)
        //for (int i = 0; int i < usdaResponse1.data.Length; int i++)
        //for (int i = 0; i < usdaResponse2.data.Length - 1; i++)
        for (int i = 0; i < sortedDataObjects1.Count; i++)
            //for (int i = 0; i < dataObjects1.Count; i++)
        {
            //if (usdaResponse1.data[i].agg_level_desc != "NATIONAL" && usdaResponse1.data[i].reference_period_desc == week)
            //if (sortedDataObjects1[i].agg_level_desc != "NATIONAL" && sortedDataObjects1[i].reference_period_desc == week)
            for (int j = 0; j < sortedDataObjects2.Count; j++)
            {
                if (sortedDataObjects1[i].state_alpha == sortedDataObjects2[j].state_alpha)
                {
                    summedDataObjects.Add(new Datum
                    {
                        prodn_practice_desc = sortedDataObjects1[i].prodn_practice_desc,
                        domain_desc = sortedDataObjects1[i].domain_desc,
                        county_name = sortedDataObjects1[i].county_name,
                        freq_desc = sortedDataObjects1[i].freq_desc,
                        begin_code = sortedDataObjects1[i].begin_code,
                        watershed_code = sortedDataObjects1[i].watershed_code,
                        end_code = sortedDataObjects1[i].end_code,
                        state_alpha = sortedDataObjects1[i].state_alpha,
                        agg_level_desc = sortedDataObjects1[i].agg_level_desc,
                        CV = sortedDataObjects1[i].CV,
                        state_ansi = sortedDataObjects1[i].state_ansi,
                        util_practice_desc = sortedDataObjects1[i].util_practice_desc,
                        region_desc = sortedDataObjects1[i].region_desc,
                        state_fips_code = sortedDataObjects1[i].state_fips_code,
                        county_code = sortedDataObjects1[i].county_code,
                        week_ending = sortedDataObjects1[i].week_ending,
                        year = sortedDataObjects1[i].year,
                        watershed_desc = sortedDataObjects1[i].watershed_desc,
                        unit_desc = sortedDataObjects1[i].unit_desc,
                        country_name = sortedDataObjects1[i].country_name,
                        domaincat_desc = sortedDataObjects1[i].domaincat_desc,
                        location_desc = sortedDataObjects1[i].location_desc,
                        zip_5 = sortedDataObjects1[i].zip_5,
                        group_desc = sortedDataObjects1[i].group_desc,
                        load_time = sortedDataObjects1[i].load_time,
                        //Value = (Int32.Parse(dataObjects1.Where(x => x.state_alpha == "AR").FirstOrDefault().Value) - (Int32.Parse(dataObjects2.Where(x => x.state_alpha == "AR").FirstOrDefault().Value))).ToString(),
                        Value = (Int32.Parse(sortedDataObjects1.Where(x => x.state_alpha == sortedDataObjects2[i].state_alpha).FirstOrDefault().Value) - (Int32.Parse(sortedDataObjects2.Where(x => x.state_alpha == sortedDataObjects1[i].state_alpha).FirstOrDefault().Value))).ToString(),
                        asd_desc = sortedDataObjects1[i].asd_desc,
                        county_ansi = sortedDataObjects1[i].county_ansi,
                        asd_code = sortedDataObjects1[i].asd_code,
                        commodity_desc = sortedDataObjects1[i].commodity_desc,
                        statisticcat_desc = sortedDataObjects1[i].statisticcat_desc,
                        congr_district_code = sortedDataObjects1[i].congr_district_code,
                        state_name = sortedDataObjects1[i].state_name,
                        reference_period_desc = sortedDataObjects1[i].reference_period_desc,
                        source_desc = sortedDataObjects1[i].source_desc,
                        class_desc = sortedDataObjects1[i].class_desc,
                        sector_desc = sortedDataObjects1[i].sector_desc,
                        country_code = sortedDataObjects1[i].country_code,
                        //short_desc = sortedDataObjects1[i].short_desc
                        short_desc = $"{year} minus five year average"
                    }

                    );
                }
            }
        }
        //return Int32.Parse(summedDataObjects[1].Value);
        return summedDataObjects;
    }


    private string BuildUsdaUrl(string metric, string commodity, string year, string short_desc)
    {
        _usdaConfig.ApiKey = "C2ADF26B-BD8D-328A-968F-2F175A287144";
        switch (metric)
        {
            case "AREA PLANTED":
                short_desc = $"{commodity} - ACRES PLANTED";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&statisticcat_desc={metric}&unit_desc=ACRES&year__LIKE={year}&commodity_desc={commodity}";
                break;
            case "AREA HARVESTED":
                url = $"{baseUrl}{_usdaConfig.ApiKey}&commodity_desc={commodity}&statistic_cat_desc={metric}&unit_desc=ACRES&year__LIKE={year}";
                break;
            case "CONDITION":
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&commodity_desc={commodity}&short_desc={short_desc}&statisticcat_desc={metric}";
                break;
            case "ETHANOL USAGE":
                short_desc = $"{commodity}, FOR FUEL ALCOHOL - USAGE, MEASURED IN BU";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";
                break;
            case "PRODUCTION":
                if (commodity == "CORN")
                {
                    short_desc = "CORN, GRAIN - PRODUCTION, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&commodity={commodity}&metric={metric}&short_desc={short_desc}";
                }
                else
                {
                    short_desc = "SOYBEANS - PRODUCTION, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&commodity={commodity}&metric={metric}&short_desc={short_desc}";
                }
                break;
            case "PROGRESS":
                short_desc = $"{commodity} - PROGRESS, MEASURED IN PCT EMERGED";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";
                break;
            case "RESIDUAL USAGE":
                short_desc = "CORN, FOR OTHER PRODUCTS (EXCL ALCOHOL) - USAGE, MEASURED IN BU";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";
                break;
            case "STOCKS":
                if (commodity == "CORN")
                {
                    short_desc = "CORN, GRAIN - PRODUCTION, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";
                }
                else
                {
                    short_desc = "SOYBEANS - STOCKS, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";
                }
                break;
            case "PROGRESS, 5 YEAR AVG, MEASURED IN PCT PLANTED":
                short_desc = $"{commodity} - PROGRESS, 5 YEAR AVG, MEASURED IN PCT PLANTED";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";
                break;
            case "CONDITION, 5 YEAR AVG, MEASURED IN PCT EXCELLENT":
                short_desc = $"{commodity} - CONDITION, 5 YEAR AVG, MEASURED IN PCT EXCELLENT";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&metric={metric}&short_desc={short_desc}";
                break;
            default:
                break;
        }

        return url;
    }

    public async Task<List<Datum>> GetUsdaDataObjectsRefactoredMultiYear(string metric, string commodity, string year, string short_desc)
    {
        url = BuildUsdaUrlMultiYear(metric, commodity, year, short_desc);

        var dataObjects = new List<Datum>();
        var sortedDataObjects = new List<Datum>();

        var client = _httpFactory.CreateClient();
        var response = await client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var usdaResponse = JsonSerializer.Deserialize<UsdaInfo>(json);
        foreach (var item in usdaResponse.data)
        {
            if (item.agg_level_desc == "NATIONAL")
            {
                dataObjects.Add(new Datum
                {
                    prodn_practice_desc = item.prodn_practice_desc,
                    domain_desc = item.domain_desc,
                    county_name = item.county_name,
                    freq_desc = item.freq_desc,
                    begin_code = item.begin_code,
                    watershed_code = item.watershed_code,
                    end_code = item.end_code,
                    state_alpha = item.state_alpha,
                    agg_level_desc = item.agg_level_desc,
                    CV = item.CV,
                    state_ansi = item.state_ansi,
                    util_practice_desc = item.util_practice_desc,
                    region_desc = item.region_desc,
                    state_fips_code = item.state_fips_code,
                    county_code = item.county_code,
                    week_ending = item.week_ending,
                    year = item.year,
                    watershed_desc = item.watershed_desc,
                    unit_desc = item.unit_desc,
                    country_name = item.country_name,
                    domaincat_desc = item.domaincat_desc,
                    location_desc = item.location_desc,
                    zip_5 = item.zip_5,
                    group_desc = item.group_desc,
                    load_time = item.load_time,
                    Value = item.Value,
                    asd_desc = item.asd_desc,
                    county_ansi = item.county_ansi,
                    asd_code = item.asd_code,
                    commodity_desc = item.commodity_desc,
                    statisticcat_desc = item.statisticcat_desc,
                    congr_district_code = item.congr_district_code,
                    state_name = item.state_name,
                    reference_period_desc = item.reference_period_desc,
                    source_desc = item.source_desc,
                    class_desc = item.class_desc,
                    sector_desc = item.sector_desc,
                    country_code = item.country_code,
                    short_desc = item.short_desc
                }
                );
            }
        }

        // TODO: add filtering (maybe a switch case grabbed from front end...)

        sortedDataObjects = dataObjects.AsEnumerable()
            .Where(x => x.domain_desc == "TOTAL" || x.location_desc == "US TOTAL")
            .OrderByDescending(x => x.year)
            // TODO: consider filtering by reference_period_desc
            .ThenByDescending(x => x.load_time)
            .ToList();
        return sortedDataObjects;
    }



    private string BuildUsdaUrlMultiYear(string metric, string commodity, string year, string short_desc)
    {
        _usdaConfig.ApiKey = "C2ADF26B-BD8D-328A-968F-2F175A287144";

        switch (metric)
        {
            case "AREA PLANTED":
                if (year.Contains(','))
                {
                    //year.Substring(4, 4).
                }
                short_desc = $"{commodity} - ACRES PLANTED";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&statisticcat_desc={metric}&unit_desc=ACRES&year__LIKE={year}&commodity_desc={commodity}";
                break;
            case "AREA HARVESTED":
                url = $"{baseUrl}{_usdaConfig.ApiKey}&commodity_desc={commodity}&statistic_cat_desc={metric}&unit_desc=ACRES&year__LIKE={year}";
                break;
            case "CONDITION":
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&commodity_desc={commodity}&short_desc={short_desc}&statisticcat_desc={metric}";
                break;
            case "ETHANOL USAGE":
                short_desc = $"{commodity}, FOR FUEL ALCOHOL - USAGE, MEASURED IN BU";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";
                break;
            case "PRODUCTION":
                if (commodity == "CORN")
                {
                    short_desc = "CORN, GRAIN - PRODUCTION, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&commodity={commodity}&metric={metric}&short_desc={short_desc}";
                }
                else
                {
                    short_desc = "SOYBEANS - PRODUCTION, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&commodity={commodity}&metric={metric}&short_desc={short_desc}";
                }
                break;
            case "PROGRESS":
                short_desc = $"{commodity} - PROGRESS, MEASURED IN PCT EMERGED";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";
                break;
            case "RESIDUAL USAGE":
                short_desc = "CORN, FOR OTHER PRODUCTS (EXCL ALCOHOL) - USAGE, MEASURED IN BU";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKEe={year}&short_desc={short_desc}";
                break;
            case "STOCKS":
                if (commodity == "CORN")
                {
                    short_desc = "CORN, GRAIN - PRODUCTION, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";
                }
                else
                {
                    short_desc = "SOYBEANS - STOCKS, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__LIKE={year}&short_desc={short_desc}";
                }
                break;
            default:
                break;
        }

        return url;
    }

    public async Task<List<Datum>> GetUsdaDataObjectsStates(string metric, string commodity, string year, string short_desc)
    {
        //url = $"{baseUrl}{_usdaConfig.ApiKey}&statisticcat_desc=PROGRESS&year__LIKE={year}&commodity_desc=CORN&short_desc=CORN%20-%20PROGRESS,%20MEASURED%20IN%20PCT%20EMERGED";
        //url = $"{baseUrl}{_usdaConfig.ApiKey}&statisticcat_desc={metric}&year__LIKE={year}&commodity_desc={commodity}&short_desc={short_desc}";
        url = BuildUsdaUrl(metric, commodity, year, short_desc);

        var dataObjects = new List<Datum>();
        var sortedDataObjects = new List<Datum>();

        var client = _httpFactory.CreateClient();
        var response = await client.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        var usdaResponse = JsonSerializer.Deserialize<UsdaInfo>(json);
        foreach (var item in usdaResponse.data)
        {
            if (item.agg_level_desc != "NATIONAL")
            {
                dataObjects.Add(new Datum
                {
                    prodn_practice_desc = item.prodn_practice_desc,
                    domain_desc = item.domain_desc,
                    county_name = item.county_name,
                    freq_desc = item.freq_desc,
                    begin_code = item.begin_code,
                    watershed_code = item.watershed_code,
                    end_code = item.end_code,
                    state_alpha = item.state_alpha,
                    agg_level_desc = item.agg_level_desc,
                    CV = item.CV,
                    state_ansi = item.state_ansi,
                    util_practice_desc = item.util_practice_desc,
                    region_desc = item.region_desc,
                    state_fips_code = item.state_fips_code,
                    county_code = item.county_code,
                    week_ending = item.week_ending,
                    year = item.year,
                    watershed_desc = item.watershed_desc,
                    unit_desc = item.unit_desc,
                    country_name = item.country_name,
                    domaincat_desc = item.domaincat_desc,
                    location_desc = item.location_desc,
                    zip_5 = item.zip_5,
                    group_desc = item.group_desc,
                    load_time = item.load_time,
                    Value = item.Value,
                    asd_desc = item.asd_desc,
                    county_ansi = item.county_ansi,
                    asd_code = item.asd_code,
                    commodity_desc = item.commodity_desc,
                    statisticcat_desc = item.statisticcat_desc,
                    congr_district_code = item.congr_district_code,
                    state_name = item.state_name,
                    reference_period_desc = item.reference_period_desc,
                    source_desc = item.source_desc,
                    class_desc = item.class_desc,
                    sector_desc = item.sector_desc,
                    country_code = item.country_code,
                    short_desc = item.short_desc
                }
                );
            }
        }

        // TODO: add filtering (maybe a switch case grabbed from front end...)

        sortedDataObjects = dataObjects.AsEnumerable()
            //.Where(x => x.domain_desc == "TOTAL" || x.location_desc == "US TOTAL")
            .Where(x => x.source_desc != "CENSUS")
            .OrderByDescending(x => x.year)
            // TODO: consider filtering by reference_period_desc
            .ThenBy(x => x.load_time)
            .ToList();
        return sortedDataObjects;
    }

    public List<Datum> GetUsdaDataObjectsByState(string metric, string commodity, string year, string short_desc, string stateAlpha)
    {
        switch (metric)
        {
            case "AREA PLANTED":
                short_desc = $"{commodity} - ACRES PLANTED";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&statisticcat_desc={metric}&unit_desc=ACRES&year__GE={year}&commodity_desc={commodity}";
                break;
            case "AREA HARVESTED":
                url = $"{baseUrl}{_usdaConfig.ApiKey}&commodity_desc={commodity}&statistic_cat_desc={metric}&unit_desc=ACRES&year__GE={year}";
                break;
            case "CONDITION":
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&short_desc={commodity}{short_desc}&commodity_desc={commodity}&statisticcat_desc={metric}";
                break;
            case "ETHANOL USAGE":
                short_desc = $"{commodity}, FOR FUEL ALCOHOL - USAGE, MEASURED IN BU";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&short_desc={short_desc}";
                break;
            case "PRODUCTION":
                if (commodity == "CORN")
                {
                    short_desc = "CORN, GRAIN - PRODUCTION, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&commodity={commodity}&metric={metric}";
                }
                else
                {
                    short_desc = "SOYBEANS - PRODUCTION, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&commodity={commodity}&metric={metric}";
                }
                break;
            case "PROGRESS":
                short_desc = $"{commodity} - PROGRESS, MEASURED IN PCT EMERGED";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&short_desc={short_desc}";
                break;
            case "RESIDUAL USAGE":
                short_desc = "CORN, FOR OTHER PRODUCTS (EXCL ALCOHOL) - USAGE, MEASURED IN BU";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&short_desc={short_desc}";
                break;
            case "STOCKS":
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&short_desc={short_desc}";
                break;
            default:
                break;
        }

        var dataObjects = new List<Datum>();
        var filteredDataObjects = new List<Datum>();

        var client = _httpFactory.CreateClient();

        var response = client.GetAsync(url).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var usdaResponse = JsonSerializer.Deserialize<UsdaInfo>(json);
            foreach (var item in usdaResponse.data)
            {
                dataObjects.Add(new Datum
                {
                    prodn_practice_desc = item.prodn_practice_desc,
                    domain_desc = item.domain_desc,
                    county_name = item.county_name,
                    freq_desc = item.freq_desc,
                    begin_code = item.begin_code,
                    watershed_code = item.watershed_code,
                    end_code = item.end_code,
                    state_alpha = item.state_alpha,
                    agg_level_desc = item.agg_level_desc,
                    CV = item.CV,
                    state_ansi = item.state_ansi,
                    util_practice_desc = item.util_practice_desc,
                    region_desc = item.region_desc,
                    state_fips_code = item.state_fips_code,
                    county_code = item.county_code,
                    week_ending = item.week_ending,
                    year = item.year,
                    watershed_desc = item.watershed_desc,
                    unit_desc = item.unit_desc,
                    country_name = item.country_name,
                    domaincat_desc = item.domaincat_desc,
                    location_desc = item.location_desc,
                    zip_5 = item.zip_5,
                    group_desc = item.group_desc,
                    load_time = item.load_time,
                    Value = item.Value,
                    asd_desc = item.asd_desc,
                    county_ansi = item.county_ansi,
                    asd_code = item.asd_code,
                    commodity_desc = item.commodity_desc,
                    statisticcat_desc = item.statisticcat_desc,
                    congr_district_code = item.congr_district_code,
                    state_name = item.state_name,
                    reference_period_desc = item.reference_period_desc,
                    source_desc = item.source_desc,
                    class_desc = item.class_desc,
                    sector_desc = item.sector_desc,
                    country_code = item.country_code,
                    short_desc = item.short_desc
                }
                );
            }

        foreach (var item in dataObjects)
        {
            if (item.state_alpha.Contains(stateAlpha))
            {
                filteredDataObjects.Add(item);
            }
        }
        //return (List<Datum>)dataObjects.OrderBy(x => x.load_time);
        //return dataObjects;
        filteredDataObjects.OrderByDescending(x => x.county_name).ToList();
        return filteredDataObjects;
    }

    public List<Datum> GetUsdaDataObjectsNoParams()
    {
        string url = $"{baseUrl}key=C2ADF26B-BD8D-328A-968F-2F175A287144&statisticcat_desc=AREA PLANTED&unit_desc=ACRES&year__GE=2020&commodity_desc=CORN";
        var dataObjects = new List<Datum>();

        var client = _httpFactory.CreateClient();

        var response = client.GetAsync(url).Result;
        var json = response.Content.ReadAsStringAsync().Result;
        var usdaResponse = JsonSerializer.Deserialize<UsdaInfo>(json);
        foreach (var item in usdaResponse.data)
        {
            dataObjects.Add(new Datum
            {
                prodn_practice_desc = item.prodn_practice_desc,
                domain_desc = item.domain_desc,
                county_name = item.county_name,
                freq_desc = item.freq_desc,
                begin_code = item.begin_code,
                watershed_code = item.watershed_code,
                end_code = item.end_code,
                state_alpha = item.state_alpha,
                agg_level_desc = item.agg_level_desc,
                CV = item.CV,
                state_ansi = item.state_ansi,
                util_practice_desc = item.util_practice_desc,
                region_desc = item.region_desc,
                state_fips_code = item.state_fips_code,
                county_code = item.county_code,
                week_ending = item.week_ending,
                year = item.year,
                watershed_desc = item.watershed_desc,
                unit_desc = item.unit_desc,
                country_name = item.country_name,
                domaincat_desc = item.domaincat_desc,
                location_desc = item.location_desc,
                zip_5 = item.zip_5,
                group_desc = item.group_desc,
                load_time = item.load_time,
                Value = item.Value,
                asd_desc = item.asd_desc,
                county_ansi = item.county_ansi,
                asd_code = item.asd_code,
                commodity_desc = item.commodity_desc,
                statisticcat_desc = item.statisticcat_desc,
                congr_district_code = item.congr_district_code,
                state_name = item.state_name,
                reference_period_desc = item.reference_period_desc,
                source_desc = item.source_desc,
                class_desc = item.class_desc,
                sector_desc = item.sector_desc,
                country_code = item.country_code,
                short_desc = item.short_desc
            }
            );
        }


        return dataObjects;
    }

    //to be deleted
    public List<Datum> GetUsdaDataObjectsOld(string metric, string commodity, string year)
    {
        string url = $"{baseUrl}{_usdaConfig.ApiKey}&statisticcat_desc={metric}&unit_desc=ACRES&year__GE={year}&commodity_desc={commodity}";
        var dataObjects = new List<Datum>();

        var client = _httpFactory.CreateClient();

        var response = client.GetAsync(url).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var usdaResponse = JsonSerializer.Deserialize<UsdaInfo>(json);
            foreach (var item in usdaResponse.data)
            {
                dataObjects.Add(new Datum
                {
                    prodn_practice_desc = item.prodn_practice_desc,
                    domain_desc = item.domain_desc,
                    county_name = item.county_name,
                    freq_desc = item.freq_desc,
                    begin_code = item.begin_code,
                    watershed_code = item.watershed_code,
                    end_code = item.end_code,
                    state_alpha = item.state_alpha,
                    agg_level_desc = item.agg_level_desc,
                    CV = item.CV,
                    state_ansi = item.state_ansi,
                    util_practice_desc = item.util_practice_desc,
                    region_desc = item.region_desc,
                    state_fips_code = item.state_fips_code,
                    county_code = item.county_code,
                    week_ending = item.week_ending,
                    year = item.year,
                    watershed_desc = item.watershed_desc,
                    unit_desc = item.unit_desc,
                    country_name = item.country_name,
                    domaincat_desc = item.domaincat_desc,
                    location_desc = item.location_desc,
                    zip_5 = item.zip_5,
                    group_desc = item.group_desc,
                    load_time = item.load_time,
                    Value = item.Value,
                    asd_desc = item.asd_desc,
                    county_ansi = item.county_ansi,
                    asd_code = item.asd_code,
                    commodity_desc = item.commodity_desc,
                    statisticcat_desc = item.statisticcat_desc,
                    congr_district_code = item.congr_district_code,
                    state_name = item.state_name,
                    reference_period_desc = item.reference_period_desc,
                    source_desc = item.source_desc,
                    class_desc = item.class_desc,
                    sector_desc = item.sector_desc,
                    country_code = item.country_code,
                    short_desc = item.short_desc
                }
                );
            }

        return dataObjects;
    }

    public List<Datum> GetUsdaDataObjects(string metric, string commodity, string year, string short_desc)
    {
        switch (metric)
        {
            case "AREA PLANTED":
                short_desc = $"{commodity} - ACRES PLANTED";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&statisticcat_desc={metric}&unit_desc=ACRES&year__GE={year}&commodity_desc={commodity}";
                break;
            case "AREA HARVESTED":
                url = $"{baseUrl}{_usdaConfig.ApiKey}&commodity_desc={commodity}&statistic_cat_desc={metric}&unit_desc=ACRES&year__GE={year}";
                break;
            case "CONDITION":
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&commodity_desc={commodity}&short_desc={short_desc}&statisticcat_desc={metric}";
                break;
            case "ETHANOL USAGE":
                short_desc = $"{commodity}, FOR FUEL ALCOHOL - USAGE, MEASURED IN BU";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&short_desc={short_desc}";
                break;
            case "PRODUCTION":
                if (commodity == "CORN")
                {
                    short_desc = "CORN, GRAIN - PRODUCTION, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&commodity={commodity}&metric={metric}&short_desc={short_desc}";
                }
                else
                {
                    short_desc = "SOYBEANS - PRODUCTION, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&commodity={commodity}&metric={metric}&short_desc={short_desc}";
                }
                break;
            case "PROGRESS":
                short_desc = $"{commodity} - PROGRESS, MEASURED IN PCT EMERGED";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&short_desc={short_desc}";
                break;
            case "RESIDUAL USAGE":
                short_desc = "CORN, FOR OTHER PRODUCTS (EXCL ALCOHOL) - USAGE, MEASURED IN BU";
                url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&short_desc={short_desc}";
                break;
            case "STOCKS":
                if (commodity == "CORN")
                {
                    short_desc = "CORN, GRAIN - PRODUCTION, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&short_desc={short_desc}";
                }
                else
                {
                    short_desc = "SOYBEANS - STOCKS, MEASURED IN BU";
                    url = $"{baseUrl}{_usdaConfig.ApiKey}&year__GE={year}&short_desc={short_desc}";
                }
                break;
            default:
                break;
        }

        var dataObjects = new List<Datum>();
        var sortedDataObjects = new List<Datum>();

        var client = _httpFactory.CreateClient();

        var response = client.GetAsync(url).Result;
            var json = response.Content.ReadAsStringAsync().Result;
            var usdaResponse = JsonSerializer.Deserialize<UsdaInfo>(json);
            foreach (var item in usdaResponse.data)
            {
                if (item.agg_level_desc == "NATIONAL")
                {
                    dataObjects.Add(new Datum
                    {
                        prodn_practice_desc = item.prodn_practice_desc,
                        domain_desc = item.domain_desc,
                        county_name = item.county_name,
                        freq_desc = item.freq_desc,
                        begin_code = item.begin_code,
                        watershed_code = item.watershed_code,
                        end_code = item.end_code,
                        state_alpha = item.state_alpha,
                        agg_level_desc = item.agg_level_desc,
                        CV = item.CV,
                        state_ansi = item.state_ansi,
                        util_practice_desc = item.util_practice_desc,
                        region_desc = item.region_desc,
                        state_fips_code = item.state_fips_code,
                        county_code = item.county_code,
                        week_ending = item.week_ending,
                        year = item.year,
                        watershed_desc = item.watershed_desc,
                        unit_desc = item.unit_desc,
                        country_name = item.country_name,
                        domaincat_desc = item.domaincat_desc,
                        location_desc = item.location_desc,
                        zip_5 = item.zip_5,
                        group_desc = item.group_desc,
                        load_time = item.load_time,
                        Value = item.Value,
                        asd_desc = item.asd_desc,
                        county_ansi = item.county_ansi,
                        asd_code = item.asd_code,
                        commodity_desc = item.commodity_desc,
                        statisticcat_desc = item.statisticcat_desc,
                        congr_district_code = item.congr_district_code,
                        state_name = item.state_name,
                        reference_period_desc = item.reference_period_desc,
                        source_desc = item.source_desc,
                        class_desc = item.class_desc,
                        sector_desc = item.sector_desc,
                        country_code = item.country_code,
                        short_desc = item.short_desc
                    }
                    );
                }
            }
        

        //foreach (var item in dataObjects)
        //{
        //    if (item.reference_period_desc.Contains("ACREAGE"));
        //    filteredDataObjects.Add(item);
        //}
        sortedDataObjects = dataObjects.AsEnumerable()
            .Where(x => x.domain_desc == "TOTAL" || x.location_desc == "US TOTAL")
            .OrderByDescending(x => x.year)
            .ThenByDescending(x => x.load_time)
            .ToList();
        //return dataObjects;
        return sortedDataObjects;
    }

    //public async Task<IActionResult> GetUsdaData(string Metrick, string Commodity, string Year)
    //public async List<string> GetUsdaData(string Metrick, string Commodity, string Year)
    //{
    //    var apiUrl = $"https://quickstats.nass.usda.gov/api/api_GET/?key={_usdaConfig.ApiKey}&statisticcat_desc={Metric}&unit_desc=ACRES&year__GE={Year}&commodity_desc={Commodity}";

    //    HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

    //    if (response.IsSuccessStatusCode)
    //    {
    //        string data = await response.Content.ReadAsStringAsync();
    //        //string data = await response.Content.ReadFromJsonAsync<string>();
    //        var json = response.Content.ReadAsStringAsync().Result;

    //        //dataSet = JsonSerializer.Deserialize<UsdaInfo>(json);

    //        return data;
    //    }
    //    else
    //    {
    //        return (int)response.StatusCode;
    //    }
    //}



    //public List<UsdaInfo> GetUsdaInfos()
    //{
    //    //string url = $"https://api.openweathermap.org/data/2.5/forecast?q={location}&appid={_openWeatherConfig.ApiKey}&units={unit}";
    //    string url = $"https://quickstats.nass.usda.gov/api/api_GET/?key={_usdaConfig.ApiKey}&statisticcat_desc=AREA PLANTED&unit_desc=ACRES&year__GE=2020/";
    //    var usdaInfos = new List<UsdaInfo>();

    //    using (HttpClient client = new HttpClient())
    //    {
    //        var response = client.GetAsync(url).Result;
    //        var json = response.Content.ReadAsStringAsync().Result;
    //        var usdaInfoResponse = JsonSerializer.Deserialize<UsdaInfoResponse>(json);
    //        foreach (var usdaInfo in usdaInfoResponse.UsdaInfos)
    //        {
    //            usdaInfos.Add(new UsdaInfo
    //            {
    //                data =
    //                [
    //                    new Datum
    //                    {
    //                        state_ansi = usdaInfo.data.FirstOrDefault().state_ansi,
    //                        congr_district_code = usdaInfo.data.FirstOrDefault().congr_district_code
    //                    }
    //                ]
    //            });;
    //        }
    //    }

    //    return usdaInfos;
    //}
}
