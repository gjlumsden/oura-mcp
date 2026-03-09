using System.Text.Json;
using FluentAssertions;
using OuraMcp.OuraClient.Models;

namespace OuraMcp.Tests.OuraClient;

public class ModelDeserializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };

    [Fact]
    public void PersonalInfo_Deserializes_AllFields()
    {
        const string json = """
            {"id":"abc","age":30,"weight":70.5,"height":175.0,"biological_sex":"male","email":"test@test.com"}
            """;

        var result = JsonSerializer.Deserialize<PersonalInfo>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("abc");
        result.Age.Should().Be(30);
        result.Weight.Should().Be(70.5);
        result.Height.Should().Be(175.0);
        result.BiologicalSex.Should().Be("male");
        result.Email.Should().Be("test@test.com");
    }

    [Fact]
    public void DailySleep_Deserializes_WithNestedContributors()
    {
        const string json = """
            {
                "id": "sleep-1",
                "score": 85,
                "day": "2024-01-15",
                "timestamp": "2024-01-15T22:30:00+00:00",
                "contributors": {
                    "deep_sleep": 80,
                    "efficiency": 90,
                    "latency": 70,
                    "rem_sleep": 85,
                    "restfulness": 75,
                    "timing": 88,
                    "total_sleep": 92
                }
            }
            """;

        var result = JsonSerializer.Deserialize<DailySleep>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("sleep-1");
        result.Score.Should().Be(85);
        result.Day.Should().Be(new DateOnly(2024, 1, 15));
        result.Timestamp.Should().Be(DateTimeOffset.Parse("2024-01-15T22:30:00+00:00"));
        result.Contributors.Should().NotBeNull();
        result.Contributors!.DeepSleep.Should().Be(80);
        result.Contributors.Efficiency.Should().Be(90);
        result.Contributors.Latency.Should().Be(70);
        result.Contributors.RemSleep.Should().Be(85);
        result.Contributors.Restfulness.Should().Be(75);
        result.Contributors.Timing.Should().Be(88);
        result.Contributors.TotalSleep.Should().Be(92);
    }

    [Fact]
    public void DailyActivity_Deserializes_CoreFields()
    {
        const string json = """
            {
                "id": "activity-1",
                "score": 78,
                "steps": 8500,
                "total_calories": 2200,
                "active_calories": 450,
                "day": "2024-01-15",
                "timestamp": "2024-01-15T00:00:00+00:00"
            }
            """;

        var result = JsonSerializer.Deserialize<DailyActivity>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("activity-1");
        result.Score.Should().Be(78);
        result.Steps.Should().Be(8500);
        result.TotalCalories.Should().Be(2200);
        result.ActiveCalories.Should().Be(450);
        result.Day.Should().Be(new DateOnly(2024, 1, 15));
        result.Timestamp.Should().Be(DateTimeOffset.Parse("2024-01-15T00:00:00+00:00"));
    }

    [Fact]
    public void HeartRate_Deserializes_AllFields()
    {
        const string json = """
            {"bpm":72,"source":"awake","timestamp":"2024-01-01T12:00:00+00:00"}
            """;

        var result = JsonSerializer.Deserialize<HeartRate>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Bpm.Should().Be(72);
        result.Source.Should().Be("awake");
        result.Timestamp.Should().Be("2024-01-01T12:00:00+00:00");
    }

    [Fact]
    public void OuraCollectionResponse_Deserializes_WithDataAndNextToken()
    {
        const string json = """
            {
                "data": [
                    {"bpm":72,"source":"awake","timestamp":"2024-01-01T12:00:00+00:00"},
                    {"bpm":65,"source":"rest","timestamp":"2024-01-01T12:05:00+00:00"}
                ],
                "next_token": "abc123"
            }
            """;

        var result = JsonSerializer.Deserialize<OuraCollectionResponse<HeartRate>>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(2);
        result.Data[0].Bpm.Should().Be(72);
        result.Data[0].Source.Should().Be("awake");
        result.Data[1].Bpm.Should().Be(65);
        result.Data[1].Source.Should().Be("rest");
        result.NextToken.Should().Be("abc123");
    }

    [Fact]
    public void OuraCollectionResponse_Deserializes_WithNullNextToken()
    {
        const string json = """
            {
                "data": [
                    {"bpm":72,"source":"awake","timestamp":"2024-01-01T12:00:00+00:00"}
                ],
                "next_token": null
            }
            """;

        var result = JsonSerializer.Deserialize<OuraCollectionResponse<HeartRate>>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Data.Should().HaveCount(1);
        result.NextToken.Should().BeNull();
    }

    [Fact]
    public void UnknownJsonFields_AreIgnored_ForForwardCompatibility()
    {
        const string json = """
            {"id":"abc","age":30,"weight":70.5,"height":175.0,"biological_sex":"male","email":"test@test.com","new_future_field":"surprise","another_field":42}
            """;

        var result = JsonSerializer.Deserialize<PersonalInfo>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("abc");
        result.Age.Should().Be(30);
    }

    [Fact]
    public void MissingOptionalFields_DeserializeAsNull()
    {
        const string json = """
            {"id":"abc"}
            """;

        var result = JsonSerializer.Deserialize<PersonalInfo>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("abc");
        result.Age.Should().BeNull();
        result.Weight.Should().BeNull();
        result.Height.Should().BeNull();
        result.BiologicalSex.Should().BeNull();
        result.Email.Should().BeNull();
    }

    [Fact]
    public void DailySleep_MissingContributors_DeserializesAsNull()
    {
        const string json = """
            {"id":"sleep-1","score":85,"day":"2024-01-15","timestamp":"2024-01-15T22:30:00+00:00"}
            """;

        var result = JsonSerializer.Deserialize<DailySleep>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("sleep-1");
        result.Score.Should().Be(85);
        result.Contributors.Should().BeNull();
    }
}
