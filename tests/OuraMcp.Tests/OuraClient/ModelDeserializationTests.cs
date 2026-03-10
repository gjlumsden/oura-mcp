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
        result.Timestamp.Should().Be(DateTimeOffset.Parse("2024-01-01T12:00:00+00:00"));
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

    [Fact]
    public void RingConfiguration_Deserializes_AllFields()
    {
        const string json = """
            {
                "id": "ring-1",
                "color": "silver",
                "design": "heritage",
                "firmware_version": "2.8.12",
                "hardware_type": "gen3",
                "set_up_at": "2024-06-01T10:00:00+00:00",
                "size": 9
            }
            """;

        var result = JsonSerializer.Deserialize<RingConfiguration>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("ring-1");
        result.Color.Should().Be("silver");
        result.Design.Should().Be("heritage");
        result.FirmwareVersion.Should().Be("2.8.12");
        result.HardwareType.Should().Be("gen3");
        result.SetUpAt.Should().Be(DateTimeOffset.Parse("2024-06-01T10:00:00+00:00"));
        result.Size.Should().Be(9);
    }

    [Fact]
    public void DailyReadiness_Deserializes_WithNestedContributors()
    {
        const string json = """
            {
                "id": "readiness-1",
                "day": "2024-02-10",
                "score": 82,
                "timestamp": "2024-02-10T07:00:00+00:00",
                "temperature_deviation": 0.12,
                "temperature_trend_deviation": -0.05,
                "contributors": {
                    "activity_balance": 75,
                    "body_temperature": 88,
                    "hrv_balance": 70,
                    "previous_day_activity": 80,
                    "previous_night": 85,
                    "recovery_index": 90,
                    "resting_heart_rate": 92,
                    "sleep_balance": 78,
                    "sleep_regularity": 65
                }
            }
            """;

        var result = JsonSerializer.Deserialize<DailyReadiness>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("readiness-1");
        result.Day.Should().Be(new DateOnly(2024, 2, 10));
        result.Score.Should().Be(82);
        result.Timestamp.Should().Be(DateTimeOffset.Parse("2024-02-10T07:00:00+00:00"));
        result.TemperatureDeviation.Should().Be(0.12);
        result.TemperatureTrendDeviation.Should().Be(-0.05);
        result.Contributors.Should().NotBeNull();
        result.Contributors!.ActivityBalance.Should().Be(75);
        result.Contributors.BodyTemperature.Should().Be(88);
        result.Contributors.HrvBalance.Should().Be(70);
        result.Contributors.PreviousDayActivity.Should().Be(80);
        result.Contributors.PreviousNight.Should().Be(85);
        result.Contributors.RecoveryIndex.Should().Be(90);
        result.Contributors.RestingHeartRate.Should().Be(92);
        result.Contributors.SleepBalance.Should().Be(78);
        result.Contributors.SleepRegularity.Should().Be(65);
    }

    [Fact]
    public void DailyStress_Deserializes_AllFields()
    {
        const string json = """
            {
                "id": "stress-1",
                "day": "2024-03-20",
                "stress_high": 3200,
                "recovery_high": 5400,
                "day_summary": "restored"
            }
            """;

        var result = JsonSerializer.Deserialize<DailyStress>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("stress-1");
        result.Day.Should().Be(new DateOnly(2024, 3, 20));
        result.StressHigh.Should().Be(3200);
        result.RecoveryHigh.Should().Be(5400);
        result.DaySummary.Should().Be("restored");
    }

    [Fact]
    public void DailyResilience_Deserializes_WithNestedContributors()
    {
        const string json = """
            {
                "id": "resilience-1",
                "day": "2024-04-05",
                "level": "strong",
                "contributors": {
                    "sleep_recovery": 85.5,
                    "daytime_recovery": 72.3,
                    "stress": 60.1
                }
            }
            """;

        var result = JsonSerializer.Deserialize<DailyResilience>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("resilience-1");
        result.Day.Should().Be(new DateOnly(2024, 4, 5));
        result.Level.Should().Be("strong");
        result.Contributors.Should().NotBeNull();
        result.Contributors!.SleepRecovery.Should().Be(85.5);
        result.Contributors.DaytimeRecovery.Should().Be(72.3);
        result.Contributors.Stress.Should().Be(60.1);
    }

    [Fact]
    public void SleepPeriod_Deserializes_CoreFields()
    {
        const string json = """
            {
                "id": "period-1",
                "day": "2024-01-15",
                "average_heart_rate": 58.2,
                "deep_sleep_duration": 4200,
                "total_sleep_duration": 28800,
                "type": "long_sleep",
                "average_hrv": 45,
                "efficiency": 92,
                "latency": 300,
                "light_sleep_duration": 15000,
                "rem_sleep_duration": 7200,
                "time_in_bed": 31000,
                "lowest_heart_rate": 50,
                "restless_periods": 3,
                "bedtime_start": "2024-01-14T22:30:00+00:00",
                "bedtime_end": "2024-01-15T06:30:00+00:00"
            }
            """;

        var result = JsonSerializer.Deserialize<SleepPeriod>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("period-1");
        result.Day.Should().Be(new DateOnly(2024, 1, 15));
        result.AverageHeartRate.Should().Be(58.2);
        result.DeepSleepDuration.Should().Be(4200);
        result.TotalSleepDuration.Should().Be(28800);
        result.Type.Should().Be("long_sleep");
        result.AverageHrv.Should().Be(45);
        result.Efficiency.Should().Be(92);
        result.Latency.Should().Be(300);
        result.LightSleepDuration.Should().Be(15000);
        result.RemSleepDuration.Should().Be(7200);
        result.TimeInBed.Should().Be(31000);
        result.LowestHeartRate.Should().Be(50);
        result.RestlessPeriods.Should().Be(3);
        result.BedtimeStart.Should().Be(DateTimeOffset.Parse("2024-01-14T22:30:00+00:00"));
        result.BedtimeEnd.Should().Be(DateTimeOffset.Parse("2024-01-15T06:30:00+00:00"));
    }

    [Fact]
    public void SleepTime_Deserializes_WithNestedOptimalBedtime()
    {
        const string json = """
            {
                "id": "sleeptime-1",
                "day": "2024-05-10",
                "optimal_bedtime": {
                    "day_tz": -18000,
                    "end_offset": 3600,
                    "start_offset": 0
                },
                "recommendation": "improve_efficiency",
                "status": "not_enough_nights"
            }
            """;

        var result = JsonSerializer.Deserialize<SleepTime>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("sleeptime-1");
        result.Day.Should().Be(new DateOnly(2024, 5, 10));
        result.Recommendation.Should().Be("improve_efficiency");
        result.Status.Should().Be("not_enough_nights");
        result.OptimalBedtime.Should().NotBeNull();
        result.OptimalBedtime!.DayTz.Should().Be(-18000);
        result.OptimalBedtime.EndOffset.Should().Be(3600);
        result.OptimalBedtime.StartOffset.Should().Be(0);
    }

    [Fact]
    public void Workout_Deserializes_AllFields()
    {
        const string json = """
            {
                "id": "workout-1",
                "activity": "running",
                "calories": 350.5,
                "day": "2024-06-15",
                "distance": 5200.0,
                "end_datetime": "2024-06-15T08:30:00+00:00",
                "intensity": "moderate",
                "label": "Morning run",
                "source": "manual",
                "start_datetime": "2024-06-15T07:45:00+00:00"
            }
            """;

        var result = JsonSerializer.Deserialize<Workout>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("workout-1");
        result.Activity.Should().Be("running");
        result.Calories.Should().Be(350.5);
        result.Day.Should().Be(new DateOnly(2024, 6, 15));
        result.Distance.Should().Be(5200.0);
        result.EndDatetime.Should().Be(DateTimeOffset.Parse("2024-06-15T08:30:00+00:00"));
        result.Intensity.Should().Be("moderate");
        result.Label.Should().Be("Morning run");
        result.Source.Should().Be("manual");
        result.StartDatetime.Should().Be(DateTimeOffset.Parse("2024-06-15T07:45:00+00:00"));
    }

    [Fact]
    public void Session_Deserializes_AllFields()
    {
        const string json = """
            {
                "id": "session-1",
                "day": "2024-07-01",
                "start_datetime": "2024-07-01T09:00:00+00:00",
                "end_datetime": "2024-07-01T09:15:00+00:00",
                "type": "breathing",
                "mood": "good"
            }
            """;

        var result = JsonSerializer.Deserialize<Session>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("session-1");
        result.Day.Should().Be(new DateOnly(2024, 7, 1));
        result.StartDatetime.Should().Be(DateTimeOffset.Parse("2024-07-01T09:00:00+00:00"));
        result.EndDatetime.Should().Be(DateTimeOffset.Parse("2024-07-01T09:15:00+00:00"));
        result.Type.Should().Be("breathing");
        result.Mood.Should().Be("good");
    }

    [Fact]
    public void DailySpo2_Deserializes_WithNestedPercentage()
    {
        const string json = """
            {
                "id": "spo2-1",
                "day": "2024-09-01",
                "spo2_percentage": {
                    "average": 97.5
                },
                "breathing_disturbance_index": 2
            }
            """;

        var result = JsonSerializer.Deserialize<DailySpo2>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("spo2-1");
        result.Day.Should().Be(new DateOnly(2024, 9, 1));
        result.Spo2Percentage.Should().NotBeNull();
        result.Spo2Percentage!.Average.Should().Be(97.5);
        result.BreathingDisturbanceIndex.Should().Be(2);
    }

    [Fact]
    public void Vo2Max_Deserializes_AllFields()
    {
        const string json = """
            {
                "id": "vo2-1",
                "day": "2024-10-05",
                "timestamp": "2024-10-05T12:00:00+00:00",
                "vo2_max": 42.8
            }
            """;

        var result = JsonSerializer.Deserialize<Vo2Max>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("vo2-1");
        result.Day.Should().Be(new DateOnly(2024, 10, 5));
        result.Timestamp.Should().Be(DateTimeOffset.Parse("2024-10-05T12:00:00+00:00"));
        result.Vo2MaxValue.Should().Be(42.8);
    }

    [Fact]
    public void CardiovascularAge_Deserializes_AllFields()
    {
        const string json = """
            {
                "day": "2024-11-20",
                "vascular_age": 35
            }
            """;

        var result = JsonSerializer.Deserialize<CardiovascularAge>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Day.Should().Be(new DateOnly(2024, 11, 20));
        result.VascularAge.Should().Be(35);
    }

    [Fact]
    public void Tag_Deserializes_AllFields()
    {
        const string json = """
            {
                "id": "tag-1",
                "day": "2024-12-01",
                "text": "Feeling great",
                "timestamp": "2024-12-01T08:00:00+00:00",
                "tags": ["tag_generic_nocaffeine", "tag_generic_stress_low"]
            }
            """;

        var result = JsonSerializer.Deserialize<Tag>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("tag-1");
        result.Day.Should().Be(new DateOnly(2024, 12, 1));
        result.Text.Should().Be("Feeling great");
        result.Timestamp.Should().Be(DateTimeOffset.Parse("2024-12-01T08:00:00+00:00"));
        result.Tags.Should().HaveCount(2);
        result.Tags![0].Should().Be("tag_generic_nocaffeine");
        result.Tags[1].Should().Be("tag_generic_stress_low");
    }

    [Fact]
    public void EnhancedTag_Deserializes_AllFields()
    {
        const string json = """
            {
                "id": "etag-1",
                "tag_type_code": "tag_generic_nocaffeine",
                "start_time": "2024-12-01T06:00:00+00:00",
                "end_time": "2024-12-01T22:00:00+00:00",
                "start_day": "2024-12-01",
                "end_day": "2024-12-01",
                "comment": "No coffee today",
                "custom_name": "caffeine_free"
            }
            """;

        var result = JsonSerializer.Deserialize<EnhancedTag>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("etag-1");
        result.TagTypeCode.Should().Be("tag_generic_nocaffeine");
        result.StartTime.Should().Be(DateTimeOffset.Parse("2024-12-01T06:00:00+00:00"));
        result.EndTime.Should().Be(DateTimeOffset.Parse("2024-12-01T22:00:00+00:00"));
        result.StartDay.Should().Be(new DateOnly(2024, 12, 1));
        result.EndDay.Should().Be(new DateOnly(2024, 12, 1));
        result.Comment.Should().Be("No coffee today");
        result.CustomName.Should().Be("caffeine_free");
    }

    [Fact]
    public void RestModePeriod_Deserializes_WithNestedEpisodes()
    {
        const string json = """
            {
                "id": "rest-1",
                "end_day": "2024-12-10",
                "end_time": "2024-12-10T18:00:00+00:00",
                "start_day": "2024-12-08",
                "start_time": "2024-12-08T10:00:00+00:00",
                "episodes": [
                    {
                        "tags": ["sick", "fever"],
                        "timestamp": "2024-12-08T10:00:00+00:00"
                    },
                    {
                        "tags": ["recovering"],
                        "timestamp": "2024-12-09T08:00:00+00:00"
                    }
                ]
            }
            """;

        var result = JsonSerializer.Deserialize<RestModePeriod>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("rest-1");
        result.EndDay.Should().Be(new DateOnly(2024, 12, 10));
        result.EndTime.Should().Be(DateTimeOffset.Parse("2024-12-10T18:00:00+00:00"));
        result.StartDay.Should().Be(new DateOnly(2024, 12, 8));
        result.StartTime.Should().Be(DateTimeOffset.Parse("2024-12-08T10:00:00+00:00"));
        result.Episodes.Should().HaveCount(2);
        result.Episodes![0].Tags.Should().BeEquivalentTo(new[] { "sick", "fever" });
        result.Episodes[0].Timestamp.Should().Be(DateTimeOffset.Parse("2024-12-08T10:00:00+00:00"));
        result.Episodes[1].Tags.Should().BeEquivalentTo(new[] { "recovering" });
        result.Episodes[1].Timestamp.Should().Be(DateTimeOffset.Parse("2024-12-09T08:00:00+00:00"));
    }

    [Fact]
    public void SampleModel_Deserializes_AllFields()
    {
        const string json = """{"interval": 60.0, "items": [72.5, 73.0, null, 71.8], "timestamp": "2024-01-01T00:00:00+00:00"}""";

        var result = JsonSerializer.Deserialize<SampleModel>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Interval.Should().Be(60.0);
        result.Items.Should().HaveCount(4);
        result.Items![0].Should().Be(72.5);
        result.Items[1].Should().Be(73.0);
        result.Items[2].Should().BeNull();
        result.Items[3].Should().Be(71.8);
        result.Timestamp.Should().Be(DateTimeOffset.Parse("2024-01-01T00:00:00+00:00"));
    }

    [Fact]
    public void SampleModel_Deserializes_WithNullFields()
    {
        const string json = """{}""";

        var result = JsonSerializer.Deserialize<SampleModel>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Interval.Should().BeNull();
        result.Items.Should().BeNull();
        result.Timestamp.Should().BeNull();
    }
}
