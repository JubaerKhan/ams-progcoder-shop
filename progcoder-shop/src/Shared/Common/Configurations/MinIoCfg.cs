namespace Common.Configurations;

public sealed class MinIoCfg
{
    #region Constants

    public const string Section = "MinIO";

    public const string Endpoint = "Endpoint";

    /// <summary>
    /// Host:port a browser can actually reach, for building PublicURL values.
    /// Distinct from Endpoint, which is the container-network address (e.g.
    /// "minio:9000") used for the server's own calls to MinIO.
    /// </summary>
    public const string PublicEndpoint = "PublicEndpoint";

    public const string AccessKey = "AccessKey";

    public const string SecretKey = "SecretKey";

    public const string Secure = "Secure";

    #endregion

}
