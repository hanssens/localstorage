namespace LocalStorage
{
    public interface ILocalStorageConfiguration
    {
        /// <summary>
        /// Filename for the persisted state on disk (defaults to ".localstorage").
        /// </summary>
        string Filename { get; set; }
    }
}