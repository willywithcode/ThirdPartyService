namespace ThirdPartyService.Core.Analytics
{
    /// <summary>
    /// Labels the player for an analytics provider, so every later event can be split by who sent
    /// it (a QA device, a payer, a build the player installed). A provider keeps the last value per
    /// name; setting the same value again is harmless.
    /// </summary>
    public interface IUserPropertyService
    {
        void SetUserProperty(string name, string value);
    }
}
