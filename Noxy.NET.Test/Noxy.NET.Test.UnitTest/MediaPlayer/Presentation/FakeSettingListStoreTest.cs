using Noxy.NET.MediaPlayer.Domain.Responses.Setting;
using Noxy.NET.MediaPlayer.Presentation.Interfaces.Stores;
using Noxy.NET.Test.MediaPlayer.Fakes;
using Noxy.NET.Test.MediaPlayer.Fixtures;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class FakeSettingListStoreTest : SettingListStoreContract
{
    protected override IListStore<ResponseSetting, string> CreateStore()
    {
        return new FakeMetadataStore().Setting;
    }
}
