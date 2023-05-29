namespace PicTrans.Helpers;
public class SecureStorageWrapper : ISecureStorage
{
    public async Task<string> GetAsync(string key)
    {
        return await SecureStorage.GetAsync(key);
    }

    public bool Remove(string key)
    {
        return SecureStorage.Remove(key);
    }

    public void RemoveAll()
    {
        SecureStorage.RemoveAll();
    }

    public async Task SetAsync(string key, string value)
    {
        await SecureStorage.SetAsync(key, value);
    }
}
