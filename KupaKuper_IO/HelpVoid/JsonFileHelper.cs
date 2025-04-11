using System.Text;
using System.Text.Json;

namespace KupaKuper_IO.HelpVoid
{
    // 读取特定属性 "string connectionString = JsonFileHelper.ReadJsonProperty<string>("config.json", "database.connectionString")"
    // 更新特定属性 "JsonFileHelper.UpdateJsonProperty("config.json", "database.connectionString", "new-connection-string")"
    /// <summary>
    /// JSON文件操作帮助类，提供JSON文件的读写、序列化和反序列化功能
    /// </summary>
    public static class JsonFileHelper
    {
        // 建议添加默认的序列化选项
        private static readonly JsonSerializerOptions DefaultOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// 写入文本文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="contents">文件内容</param>
        /// <exception cref="ArgumentNullException">路径为空时抛出</exception>
        public static void WriteAllText(string path, string? contents)
        {
            ArgumentNullException.ThrowIfNull(path);
            CreateDirectory(path);
            File.WriteAllText(path, contents);
        }

        /// <summary>
        /// 写入文本文件，使用指定编码
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="contents">文件内容</param>
        /// <param name="encoding">编码</param>
        public static void WriteAllText(string path, string? contents, Encoding encoding)
        {
            CreateDirectory(path);
            File.WriteAllText(path, contents, encoding);
        }

        /// <summary>
        /// 写入所有行到文本文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="contents">文件内容行集合</param>
        public static void WriteAllLines(string path, IEnumerable<string> contents)
        {
            CreateDirectory(path);
            File.WriteAllLines(path, contents);
        }

        /// <summary>
        /// 写入所有行到文本文件，使用指定编码
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="contents">文件内容行集合</param>
        /// <param name="encoding">编码</param>
        public static void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            CreateDirectory(path);
            File.WriteAllLines(path, contents, encoding);
        }

        /// <summary>
        /// 追加所有行到文本文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="contents">文件内容行集合</param>
        public static void AppendAllLines(string path, IEnumerable<string> contents)
        {
            CreateDirectory(path);
            File.AppendAllLines(path, contents);
        }

        /// <summary>
        /// 追加所有行到文本文件，使用指定编码
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="contents">文件内容行集合</param>
        /// <param name="encoding">编码</param>
        public static void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            CreateDirectory(path);
            File.AppendAllLines(path, contents, encoding);
        }

        /// <summary>
        /// 追加文本到文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="contents">文件内容</param>
        public static void AppendAllText(string path, string? contents)
        {
            CreateDirectory(path);
            File.AppendAllText(path, contents);
        }

        /// <summary>
        /// 追加文本到文件，使用指定编码
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="contents">文件内容</param>
        /// <param name="encoding">编码</param>
        public static void AppendAllText(string path, string? contents, Encoding encoding)
        {
            CreateDirectory(path);
            File.AppendAllText(path, contents, encoding);
        }

        /// <summary>
        /// 获取最后修改的文件
        /// </summary>
        /// <param name="paths">文件路径数组</param>
        /// <returns>最后修改的文件路径，如果没有文件则返回null</returns>
        public static string? GetLastWriteFile(string[] paths)
        {
            DateTime dateTime = DateTime.MinValue;
            string result = null;
            foreach (string text in paths)
            {
                DateTime lastWriteTime = File.GetLastWriteTime(text);
                if (lastWriteTime > dateTime)
                {
                    dateTime = lastWriteTime;
                    result = text;
                }
            }

            return result;
        }

        /// <summary>
        /// 读取配置文件，如果文件不存在则创建模板
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <param name="name">配置文件名称</param>
        /// <returns>配置对象</returns>
        /// <exception cref="JsonException">JSON反序列化失败时抛出</exception>
        /// <exception cref="NullReferenceException">反序列化结果为null时抛出</exception>
        public static T ReadConfig<T>(string path, string name = "Config") where T : class, new()
        {
            ArgumentNullException.ThrowIfNull(path);
            ArgumentNullException.ThrowIfNull(name);

            Directory.CreateDirectory(path);
            string templatePath = Path.Combine(path, $"{name}Template.json");
            string configPath = Path.Combine(path, $"{name}.json");

            // 创建模板文件
            if (!File.Exists(templatePath))
            {
                var template = new T();
                string contents = JsonSerializer.Serialize(template, DefaultOptions);
                File.WriteAllText(templatePath, contents);
            }

            // 读取配置文件
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"配置文件不存在: {configPath}");
            }

            return JsonSerializer.Deserialize<T>(
                File.ReadAllText(configPath),
                DefaultOptions) ?? throw new NullReferenceException("配置文件反序列化结果为null");
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <param name="model">配置对象</param>
        /// <param name="name">配置文件名称</param>
        public static void SaveConfig<T>(string path, T model, string name = "Config") where T : class
        {
            Directory.CreateDirectory(path);
            string contents = JsonSerializer.Serialize(model);
            File.WriteAllText(Path.Combine(path, name + ".json"), contents);
        }

        /// <summary>
        /// 读取最新的参数文件
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <returns>参数对象，如果没有找到文件则返回null</returns>
        public static T? ReadParameter<T>(string path) where T : class
        {
            Directory.CreateDirectory(path);
            string lastWriteFile = GetLastWriteFile(Directory.GetFiles(path, "*.json"));
            if (lastWriteFile == null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(File.ReadAllText(lastWriteFile, EncodingHelp.GetEncoding("GBK")));
        }

        /// <summary>
        /// 保存参数到JSON文件，使用时间戳作为文件名
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="path">保存路径</param>
        /// <param name="model">参数对象</param>
        public static void SaveParameter<T>(string path, T model) where T : class
        {
            ArgumentNullException.ThrowIfNull(path);
            ArgumentNullException.ThrowIfNull(model);

            Directory.CreateDirectory(path);
            string fileName = $"{DateTime.Now:yyyyMMddHHmmss}.json";
            string fullPath = Path.Combine(path, fileName);

            string contents = JsonSerializer.Serialize(model, DefaultOptions);
            File.WriteAllText(fullPath, contents, EncodingHelp.GetEncoding("GBK"));
        }

        /// <summary>
        /// 读取配置文件并尝试解析指定属性
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <param name="propertyPath">属性路径，使用点号分隔，例如："root.child.value"</param>
        /// <returns>属性值，如果解析失败则返回默认值</returns>
        public static T? ReadJsonProperty<T>(string path, string propertyPath)
        {
            ArgumentNullException.ThrowIfNull(path);
            ArgumentNullException.ThrowIfNull(propertyPath);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"文件不存在: {path}");
            }

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            JsonElement root = document.RootElement;

            // 按照属性路径逐级查找
            string[] properties = propertyPath.Split('.');
            JsonElement current = root;

            foreach (string property in properties)
            {
                if (!current.TryGetProperty(property, out current))
                {
                    return default;
                }
            }

            // 尝试转换为指定类型
            try
            {
                return JsonSerializer.Deserialize<T>(current.GetRawText(), DefaultOptions);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// 更新JSON文件中的指定属性
        /// </summary>
        /// <typeparam name="T">属性值类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <param name="propertyPath">属性路径，使用点号分隔</param>
        /// <param name="value">新的属性值</param>
        public static void UpdateJsonProperty<T>(string path, string propertyPath, T value)
        {
            ArgumentNullException.ThrowIfNull(path);
            ArgumentNullException.ThrowIfNull(propertyPath);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"文件不存在: {path}");
            }

            // 读取现有JSON文档
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            var root = document.RootElement.Clone();

            // 创建更新后的JSON对象
            using var ms = new MemoryStream();
            using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = true }))
            {
                UpdateJsonElementRecursive(writer, root, propertyPath.Split('.'), value, 0);
            }

            // 写回文件
            File.WriteAllBytes(path, ms.ToArray());
        }

        /// <summary>
        /// 递归更新JSON元素
        /// </summary>
        /// <typeparam name="T">属性值类型</typeparam>
        /// <param name="writer">JSON写入器</param>
        /// <param name="element">当前JSON元素</param>
        /// <param name="propertyPath">属性路径数组</param>
        /// <param name="value">新的属性值</param>
        /// <param name="depth">当前深度</param>
        private static void UpdateJsonElementRecursive<T>(
            Utf8JsonWriter writer,
            JsonElement element,
            string[] propertyPath,
            T value,
            int depth)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                writer.WriteRawValue(element.GetRawText());
                return;
            }

            writer.WriteStartObject();

            foreach (JsonProperty property in element.EnumerateObject())
            {
                writer.WritePropertyName(property.Name);

                if (property.Name == propertyPath[depth] && depth == propertyPath.Length - 1)
                {
                    // 到达目标属性，写入新值
                    JsonSerializer.Serialize(writer, value, DefaultOptions);
                }
                else if (property.Name == propertyPath[depth] && depth < propertyPath.Length - 1)
                {
                    // 递归更新子属性
                    UpdateJsonElementRecursive(writer, property.Value, propertyPath, value, depth + 1);
                }
                else
                {
                    // 保持原值不变
                    writer.WriteRawValue(property.Value.GetRawText());
                }
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// 创建文件目录
        /// </summary>
        /// <param name="fileName">文件路径</param>
        private static void CreateDirectory(string fileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName) ?? throw new ArgumentNullException());
        }
    }
}
