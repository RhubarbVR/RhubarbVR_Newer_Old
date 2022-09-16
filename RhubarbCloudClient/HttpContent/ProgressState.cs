namespace RhubarbCloudClient.HttpContent
{
	public enum ProgressState
	{
		Unknown,
		PendingDownload,
		PendingUpload,
		Dowloading,
		Uploading,
		PendingResponse,
		Done,
		Failed,
	}
}