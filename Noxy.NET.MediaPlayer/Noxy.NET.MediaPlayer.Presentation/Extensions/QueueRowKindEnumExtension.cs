using System.ComponentModel;
using Noxy.NET.MediaPlayer.Presentation.Enums;

namespace Noxy.NET.MediaPlayer.Presentation.Extensions;

public static class QueueRowKindEnumExtension
{
    extension(QueueRowKindEnum kind)
    {
        // A song, as against a heading.
        public bool IsItem
        {
            get
            {
                switch (kind)
                {
                    case QueueRowKindEnum.DividerCurrent:
                    case QueueRowKindEnum.DividerUserQueue:
                    case QueueRowKindEnum.DividerSource:
                        return false;
                    case QueueRowKindEnum.History:
                    case QueueRowKindEnum.Current:
                    case QueueRowKindEnum.UserQueue:
                    case QueueRowKindEnum.Source:
                        return true;
                    default:
                        throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(QueueRowKindEnum));
                }
            }
        }

        // A song that has not played yet: it can be dragged, moved up and removed.
        public bool IsUpcoming
        {
            get
            {
                switch (kind)
                {
                    case QueueRowKindEnum.UserQueue:
                    case QueueRowKindEnum.Source:
                        return true;
                    case QueueRowKindEnum.DividerCurrent:
                    case QueueRowKindEnum.DividerUserQueue:
                    case QueueRowKindEnum.DividerSource:
                    case QueueRowKindEnum.History:
                    case QueueRowKindEnum.Current:
                        return false;
                    default:
                        throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(QueueRowKindEnum));
                }
            }
        }
    }
}
