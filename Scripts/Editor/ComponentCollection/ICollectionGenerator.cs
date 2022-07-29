using System.Collections.Generic;

namespace GameExtension.Editor
{
    /// <summary>
    /// 集合生成器接口。
    /// </summary>
    public interface ICollectionGenerator
    {
        /// <summary>
        /// 生成 Components 代码。
        /// </summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="codeTemplate">代码模板。</param>
        /// <param name="nameSpace">命名空间。</param>
        /// <param name="className">类名。</param>
        /// <param name="fieldTypeDict">字段类型字典。</param>
        void GenerateComponentsCode(string filePath, string codeTemplate, string nameSpace, string className, Dictionary<string, string> fieldTypeDict);

        /// <summary>
        /// 生成 MonoBehaviour 代码。
        /// </summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="codeTemplate">代码模板。</param>
        /// <param name="nameSpace">命名空间。</param>
        /// <param name="className">类名。</param>
        void GenerateBehaviourCode(string filePath, string codeTemplate, string nameSpace, string className);
    }
}
