namespace Steamworks
{
    public struct PublishedFileId_t
    {
        public ulong m_PublishedFileId;
    }

    public class SteamManager
    {
        public static bool Initialized => true;
    }

    public class SteamUser
    {
        public static bool BLoggedOn() => true;
    }

    public class SteamUGC
    {
        public static uint GetNumSubscribedItems() => 0;
        public static bool GetSubscribedItems(PublishedFileId_t[] pvecPublishedFileID, uint cMaxEntries) => true;
        public static uint GetItemState(PublishedFileId_t fileId) => 0;
        public static bool DownloadItem(PublishedFileId_t fileId, bool highPriority) => true;
        public static bool GetItemInstallInfo(PublishedFileId_t fileId, out ulong punSizeOnDisk, out string pchFolder, uint cchFolderSize, out uint punTimeStamp)
        {
            punSizeOnDisk = 0;
            pchFolder = "";
            punTimeStamp = 0;
            return true;
        }
    }

    public enum EItemState
    {
        k_EItemStateNone = 0,
        k_EItemStateSubscribed = 1,
        k_EItemStateLegacyItem = 2,
        k_EItemStateInstalled = 4,
        k_EItemStateNeedsUpdate = 8,
        k_EItemStateDownloading = 16,
        k_EItemStateDownloadPending = 32
    }
} 