public class Airport
{        
    public string airportname { get; set; }
    public string city { get; set; }
    public string country { get; set; }
    public string faa { get; set; }
    public Geo geo { get; set; }
    public string icao { get; set; }
    public int id { get; set; }
    public string type { get; set; }
    public string tz { get; set; }
}
 public class Geo
{
    public double alt { get; set; }
    public double lat { get; set; }
    public double lon { get; set; }
}