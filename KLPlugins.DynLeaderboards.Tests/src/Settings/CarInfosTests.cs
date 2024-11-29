using JetBrains.Annotations;

using KLPlugins.DynLeaderboards.Settings;

using Newtonsoft.Json;

using Xunit;

namespace KLPlugins.DynLeaderboards.Tests.Settings;

[TestSubject(typeof(OverridableCarInfo))]
public class OverridableCarInfoTests {

    [Theory]
    [InlineData("""{"Overrides":null,"IsNameEnabled":true,"IsClassEnabled":false,"SimHubCarClass":"None"}""")]
    [InlineData("""{"Overrides":{"Name":null,"Manufacturer":null,"Class":null},"IsNameEnabled":false,"IsClassEnabled":true,"SimHubCarClass":"GT3"}""")]
    public void JsonRoundTrip(string json) {
        var settings = JsonConvert.DeserializeObject<OverridableCarInfo>(json);
        Assert.NotNull(settings);

        var newJson = JsonConvert.SerializeObject(settings);
        Assert.Equal(json, newJson);
    }
}

[TestSubject(typeof(CarInfo))]
public class CarInfosTests {

    [Theory]
    [InlineData("""{"Name":"Alpine A110 GT4","Manufacturer":"Alpine","Class":"GT4"}""")]
    [InlineData("""{"Name":null,"Manufacturer":null,"Class":null}""")]
    public void JsonRoundTrip(string json) {
        var settings = JsonConvert.DeserializeObject<CarInfo>(json);
        Assert.NotNull(settings);

        var newJson = JsonConvert.SerializeObject(settings);
        Assert.Equal(json, newJson);
    }
}