using Noxy.NET.Test.MediaPlayer.Fakes;

namespace Noxy.NET.Test.MediaPlayer.Presentation;

public class MetadataRecentServiceTest
{
    private readonly FakeDevicePreferenceStorage _storage = new();
    private readonly MetadataRecentService _service;

    public MetadataRecentServiceTest()
    {
        _service = new(_storage);
    }

    private static List<Guid> NewIDList(int count) => Enumerable.Range(0, count).Select(_ => Guid.CreateVersion7()).ToList();

    [Fact]
    public async Task Record_PutsTheItemFirst_AndKeepsFive()
    {
        List<Guid> idList = NewIDList(7);

        foreach (Guid id in idList) await _service.Record(MetadataKindEnum.Artist, id);

        List<Guid> result = await _service.Get(MetadataKindEnum.Artist, idList.ToHashSet());
        Assert.Equal(Enumerable.Reverse(idList).Take(5), result);
    }

    [Fact]
    public async Task Record_TheSameItemTwice_ListsItOnce()
    {
        List<Guid> idList = NewIDList(2);
        await _service.Record(MetadataKindEnum.Genre, idList[0]);
        await _service.Record(MetadataKindEnum.Genre, idList[1]);

        await _service.Record(MetadataKindEnum.Genre, idList[0]);

        List<Guid> result = await _service.Get(MetadataKindEnum.Genre, idList.ToHashSet());
        Assert.Equal([idList[0], idList[1]], result);
    }

    [Fact]
    public async Task Get_DropsIDsThatNoLongerExist()
    {
        List<Guid> idList = NewIDList(3);
        foreach (Guid id in idList) await _service.Record(MetadataKindEnum.Album, id);

        List<Guid> result = await _service.Get(MetadataKindEnum.Album, new HashSet<Guid> { idList[0], idList[2] });

        Assert.Equal([idList[2], idList[0]], result);
    }

    [Fact]
    public async Task Get_KindsDoNotShareAList()
    {
        Guid artist = Guid.CreateVersion7();
        Guid genre = Guid.CreateVersion7();
        await _service.Record(MetadataKindEnum.Artist, artist);
        await _service.Record(MetadataKindEnum.Genre, genre);
        HashSet<Guid> existingSet = [artist, genre];

        Assert.Equal([artist], await _service.Get(MetadataKindEnum.Artist, existingSet));
        Assert.Equal([genre], await _service.Get(MetadataKindEnum.Genre, existingSet));
        Assert.Empty(await _service.Get(MetadataKindEnum.Album, existingSet));
    }

    [Fact]
    public async Task Get_WhatIsStoredIsNotAList_StartsAgainWithAnEmptyList()
    {
        await _storage.Set("metadataRecentArtist", "not json");

        Assert.Empty(await _service.Get(MetadataKindEnum.Artist, new HashSet<Guid>()));
    }
}
