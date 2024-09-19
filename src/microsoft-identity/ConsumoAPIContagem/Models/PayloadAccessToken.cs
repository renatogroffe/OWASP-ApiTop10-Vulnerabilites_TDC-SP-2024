namespace ConsumoAPIContagem.Models;

public class PayloadAccessToken
{
    public string? aud { get; set; }
    public string? iss { get; set; }
    public int iat { get; set; }
    public int nbf { get; set; }
    public int exp { get; set; }
    public string? aio { get; set; }
    public string? app_displayname { get; set; }
    public string? appid { get; set; }
    public string? appidacr { get; set; }
    public string? idp { get; set; }
    public string? idtyp { get; set; }
    public string? oid { get; set; }
    public string? rh { get; set; }
    public string? sub { get; set; }
    public string? tenant_region_scope { get; set; }
    public string? tid { get; set; }
    public string? uti { get; set; }
    public string? ver { get; set; }
    public string[]? wids { get; set; }
    public int xms_tcdt { get; set; }
}
