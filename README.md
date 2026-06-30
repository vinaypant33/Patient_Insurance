# Patient Insurance

A Windows desktop app (WPF, .NET 8) for managing patient insurance: a
dashboard of claims and renewals, a patient list, and a **machine learning
premium predictor** built with [ML.NET](https://dotnet.microsoft.com/apps/machinelearning-ai/ml-dotnet).

![Premium Predictor page](docs/screenshots/predictor.png)

## Features

- **Dashboard**: patient, policy, claim and premium totals, recent claims, a
  claims-by-status chart and upcoming renewals
- **Patient Data**: every patient with their predicted yearly medical cost and
  a Low / Medium / High risk badge
- **Premium Predictor**: enter age, BMI, children, sex, region and smoking
  status to get the expected yearly cost, a suggested monthly premium and the
  risk level
- **Model accuracy card**: R², average error and RMSE measured on patients the
  model never saw while training
- Light and dark themes

![Patient Data with risk levels](docs/screenshots/patients.png)
