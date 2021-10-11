using System;

namespace Build
{
    public class PushTarget
    {
        public PushTargetType Type { get; }

        public string FeedUrl { get; }

        public Func<BuildContext, bool> IsActive { get; }


        public PushTarget(PushTargetType type, string feedUrl, Func<BuildContext, bool> isActive)
        {
            if (String.IsNullOrWhiteSpace(feedUrl))
                throw new ArgumentException("Value must not be null or whitespace", nameof(feedUrl));

            if (!Enum.IsDefined<PushTargetType>(type))
                throw new ArgumentException($"Undefined enum value '{type}'", nameof(type));

            Type = type;
            FeedUrl = feedUrl;
            IsActive = isActive ?? throw new ArgumentNullException(nameof(isActive));
        }
    }
}
