//using Microsoft.ML;
//using Microsoft.ML.Data;
//using System;

//namespace NSL.Refactoring.PropertyCompletion.Core
//{
//    public class PropertyPrediction
//    {
//        private static string ModelPath = "PropertyTypeModel.zip";

//        public class PropertyData
//        {
//            [LoadColumn(0)] public string PropertyName { get; set; }
//            [LoadColumn(1)] public string PropertyType { get; set; }
//        }

//        public class PropertyPredictionResult
//        {
//            [ColumnName("PredictedLabel")] public string PredictedType { get; set; }
//        }

//        public static void TrainModel()
//        {
//            var mlContext = new MLContext();
//            var data = mlContext.Data.LoadFromTextFile<PropertyData>("PropertyDataset.csv", separatorChar: ',', hasHeader: true);

//            // Преобразуем текст в числовые векторы
//            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(PropertyData.PropertyName))
//                .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(PropertyData.PropertyType)))
//                .Append(mlContext.Transforms.NormalizeMinMax("Features"))
//                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
//                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

//            var model = pipeline.Fit(data);

//            // Сохраняем модель
//            mlContext.Model.Save(model, data.Schema, ModelPath);
//            Console.WriteLine("Модель обучена и сохранена!");
//        }

//        public static string PredictType(string propertyName)
//        {
//            return "object";
//            var mlContext = new MLContext();
//            var model = mlContext.Model.Load(ModelPath, out var schema);
//            var predictor = mlContext.Model.CreatePredictionEngine<PropertyData, PropertyPredictionResult>(model);

//            var result = predictor.Predict(new PropertyData { PropertyName = propertyName });
//            return result.PredictedType;
//        }
//    }
//}