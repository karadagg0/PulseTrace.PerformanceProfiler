# PulseTrace

**PulseTrace** is a lightweight and extensible performance profiling library for ASP.NET Core applications.

✔️ Detects slow-performing controllers or methods  
✔️ Logs performance data in JSON format  
✔️ Works only with `[Profile]` attribute for explicit targeting  
✔️ Database-agnostic: no external data store required  
✔️ Dashboard-ready – visualize performance metrics easily

# How To Use
```csharp

  builder.Services.AddPulseTrace(options =>
  {
      options.ThresholdMilliseconds = 1000;
      options.IncludeControllers = new List<string> { "WeatherForecast" };
      options.OutputJsonPath = "C:\\Users\\ekink\\source\\repos\\PulceTrace.txt";
  });
  builder.Services.AddControllers(options =>
  {
      options.Filters.Add<PulseTracer>();
  });

 [HttpGet]
 [Profile] 
 public IEnumerable<WeatherForecast> Get()
 {
     return Enumerable.Range(1, 5).Select(index => new WeatherForecast
     {
         Date = DateTime.Now.AddDays(index),
         TemperatureC = Random.Shared.Next(-20, 55),
         Summary = Summaries[Random.Shared.Next(Summaries.Length)]
     })
     .ToArray();
 }
