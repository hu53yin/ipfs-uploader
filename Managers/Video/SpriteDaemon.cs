using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Uploader.Daemons;
using Uploader.Managers.Common;
using Uploader.Managers.Front;
using Uploader.Managers.Ipfs;
using Uploader.Models;

namespace Uploader.Managers.Video
{
    public class SpriteDaemon : BaseDaemon
    {
        public static SpriteDaemon Instance { get; private set; }

        static SpriteDaemon()
        {
            Instance = new SpriteDaemon();
            Instance.Start(VideoSettings.NbSpriteDaemon);
        }

        protected override void ProcessItem(FileItem fileItem)
        {
            // si le client a pas demandé le progress depuis plus de 20s, annuler l'opération
            if ((DateTime.UtcNow - fileItem.FileContainer.LastTimeProgressRequested).TotalSeconds > FrontSettings.MaxGetProgressCanceled)
            {
                LogManager.AddSpriteMessage("SourceFileName " + Path.GetFileName(fileItem.SourceFilePath) + " car dernier getProgress a dépassé 20s", "Annulation");
                fileItem.CancelEncode();
                return;
            }

            // sprite creation video
            if (SpriteManager.Encode(fileItem))
                IpfsDaemon.Instance.Queue(fileItem);
        }

        protected override void LogException(FileItem fileItem, Exception ex)
        {
            LogManager.AddSpriteMessage(ex.ToString(), "Exception non gérée");                        
            fileItem.SetEncodeErrorMessage("Exception non gérée");
        }

        public void Queue(FileItem fileItem, string messageIpfs)
        {
            base.Queue(fileItem, fileItem.EncodeProcess);

            fileItem.IpfsProcess.SetProgress(messageIpfs, true);
        }
    }
}