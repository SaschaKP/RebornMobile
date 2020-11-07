using System;

public abstract class DownloaderBase : IDisposable
{
    protected DownloadState downloadState;
    protected DownloadPresenter downloadPresenter;
    
    public virtual void Initialize(DownloadState downloadState, DownloadPresenter downloadPresenter)
    {
        this.downloadState = downloadState;
        this.downloadPresenter = downloadPresenter;
    }

    public virtual void Dispose()
    {
        downloadState = null;
        downloadPresenter = null;
    }
}