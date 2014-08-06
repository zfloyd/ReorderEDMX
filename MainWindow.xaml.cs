using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ReorderEDMX
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			uxSubmit.Click += uxSubmit_Click;
		}

		bool ValidationChecks()
		{
			if (!uxEDMXPath.Text.ToLower().EndsWith(".edmx"))
			{
				MessageBox.Show("Thats not an EDMX file!");
				return false;
			}
			if (!File.Exists(uxEDMXPath.Text))
			{
				MessageBox.Show("Could not find EDMX file, please try again.");
				return false;
			}
			return true;
		}

		void uxSubmit_Click(object sender, RoutedEventArgs e)
		{
			if (!ValidationChecks())
				return;
			string edmxPath = uxEDMXPath.Text;
			XDocument edmxFile = XDocument.Load(edmxPath);
			XNamespace edmxNamespace = "http://schemas.microsoft.com/ado/2009/11/edmx";
			XNamespace storageSchemaNamespace = "http://schemas.microsoft.com/ado/2009/11/edm/ssdl";
			XNamespace storeNamespace = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator";
			XNamespace schemaNamespace = "http://schemas.microsoft.com/ado/2009/11/edm";
			XNamespace mappingNamespace = "http://schemas.microsoft.com/ado/2009/11/mapping/cs";

			XElement edmxRoot = edmxFile.Element(edmxNamespace + "Edmx");
			XElement edmxRuntime = edmxRoot.Element(edmxNamespace + "Runtime");
			XElement edmxStorageSchema = edmxRuntime.Element(edmxNamespace + "StorageModels").Element(storageSchemaNamespace + "Schema");
			XElement edmxConceptualSchema = edmxRuntime.Element(edmxNamespace + "ConceptualModels").Element(schemaNamespace + "Schema");
			XElement edmxEntityContainerMapping = edmxRuntime.Element(edmxNamespace + "Mappings").Element(mappingNamespace + "Mapping").Element(mappingNamespace + "EntityContainerMapping");

			List<XElement> storageEntitySets = new List<XElement>();
			List<XElement> storageAssociationSets = new List<XElement>();
			List<XElement> storageEntityTypes = new List<XElement>();
			List<XElement> storageAssociations = new List<XElement>();
			List<XElement> storageFunctions = new List<XElement>();

			List<XElement> conceptualEntitySets = new List<XElement>();
			List<XElement> conceptualAssociationSets = new List<XElement>();
			List<XElement> conceptualFunctionImports = new List<XElement>();
			List<XElement> conceptualEntityTypes = new List<XElement>();
			List<XElement> conceptualAssociations = new List<XElement>();
			List<XElement> conceptualComplexTypes = new List<XElement>();

			List<XElement> mappingEntitySetMapping = new List<XElement>();
			List<XElement> mappingFunctionImportMapping = new List<XElement>();

			if (edmxRuntime != null)
			{
				XElement schemaStorageSchema = edmxRuntime.Element(edmxNamespace + "StorageModels").Element(storageSchemaNamespace + "Schema");
				if (schemaStorageSchema != null)
				{
					if (schemaStorageSchema.Element(storageSchemaNamespace + "EntityContainer") != null)
					{
						storageEntitySets.AddRange(schemaStorageSchema.Element(storageSchemaNamespace + "EntityContainer").Elements(storageSchemaNamespace + "EntitySet"));
						storageAssociationSets.AddRange(schemaStorageSchema.Element(storageSchemaNamespace + "EntityContainer").Elements(storageSchemaNamespace + "AssociationSet"));
					}
					storageEntityTypes.AddRange(schemaStorageSchema.Elements(storageSchemaNamespace + "EntityType"));
					storageAssociations.AddRange(schemaStorageSchema.Elements(storageSchemaNamespace + "Association"));
					storageFunctions.AddRange(schemaStorageSchema.Elements(storageSchemaNamespace + "Function"));
				}

				XElement schemaConceptualSchema = edmxRuntime.Element(edmxNamespace + "ConceptualModels").Element(schemaNamespace + "Schema");
				if (schemaConceptualSchema != null)
				{
					if (schemaConceptualSchema.Element(schemaNamespace + "EntityContainer") != null)
					{
						conceptualEntitySets.AddRange(schemaConceptualSchema.Element(schemaNamespace + "EntityContainer").Elements(schemaNamespace + "EntitySet"));
						conceptualAssociationSets.AddRange(schemaConceptualSchema.Element(schemaNamespace + "EntityContainer").Elements(schemaNamespace + "AssociationSet"));
						conceptualFunctionImports.AddRange(schemaConceptualSchema.Element(schemaNamespace + "EntityContainer").Elements(schemaNamespace + "FunctionImport"));
					}
					conceptualEntityTypes.AddRange(schemaConceptualSchema.Elements(schemaNamespace + "EntityType"));
					conceptualAssociations.AddRange(schemaConceptualSchema.Elements(schemaNamespace + "Association"));
					conceptualComplexTypes.AddRange(schemaConceptualSchema.Elements(schemaNamespace + "ComplexType"));
				}

				XElement schemaEntityContainerMapping = edmxRuntime.Element(edmxNamespace + "Mappings").Element(mappingNamespace + "Mapping").Element(mappingNamespace + "EntityContainerMapping");
				if (schemaEntityContainerMapping != null)
				{
					mappingEntitySetMapping.AddRange(schemaEntityContainerMapping.Elements(mappingNamespace + "EntitySetMapping"));
					mappingFunctionImportMapping.AddRange(schemaEntityContainerMapping.Elements(mappingNamespace + "FunctionImportMapping"));
				}
			}

			edmxStorageSchema.Elements(storageSchemaNamespace + "EntityType").Remove();
			edmxStorageSchema.Elements(storageSchemaNamespace + "Association").Remove();
			edmxStorageSchema.Elements(storageSchemaNamespace + "Function").Remove();
			XElement entityContainer = edmxStorageSchema.Element(storageSchemaNamespace + "EntityContainer");
			edmxStorageSchema.Element(storageSchemaNamespace + "EntityContainer").Remove();
			edmxStorageSchema.Add(storageEntityTypes.OrderBy(c => c.Attribute("Name").Value));
			edmxStorageSchema.Add(storageAssociations.OrderBy(c => c.Attribute("Name").Value));
			edmxStorageSchema.Add(storageFunctions.OrderBy(c => c.Attribute("Name").Value));
			entityContainer.RemoveNodes();
			entityContainer.Add(storageEntitySets.OrderBy(c => c.Attribute("Name").Value));
			entityContainer.Add(storageAssociationSets.OrderBy(c => c.Attribute("Name").Value));
			edmxStorageSchema.Add(entityContainer);

			edmxConceptualSchema.Elements(schemaNamespace + "EntityType").Remove();
			edmxConceptualSchema.Elements(schemaNamespace + "Association").Remove();
			edmxConceptualSchema.Elements(schemaNamespace + "ComplexType").Remove();
			XElement conceptualEntityContainer = edmxConceptualSchema.Element(schemaNamespace + "EntityContainer");
			edmxConceptualSchema.Element(schemaNamespace + "EntityContainer").Remove();
			edmxConceptualSchema.Add(conceptualEntityTypes.OrderBy(c => c.Attribute("Name").Value));
			edmxConceptualSchema.Add(conceptualAssociations.OrderBy(c => c.Attribute("Name").Value));
			edmxConceptualSchema.Add(conceptualComplexTypes.OrderBy(c => c.Attribute("Name").Value));
			conceptualEntityContainer.RemoveNodes();
			conceptualEntityContainer.Add(conceptualEntitySets.OrderBy(c => c.Attribute("Name").Value));
			conceptualEntityContainer.Add(conceptualAssociationSets.OrderBy(c => c.Attribute("Name").Value));
			conceptualEntityContainer.Add(conceptualFunctionImports.OrderBy(c => c.Attribute("Name").Value));
			edmxConceptualSchema.Add(conceptualEntityContainer);

			edmxEntityContainerMapping.Elements(mappingNamespace + "EntitySetMapping").Remove();
			edmxEntityContainerMapping.Elements(mappingNamespace + "FunctionImportMapping").Remove();
			edmxEntityContainerMapping.Add(mappingEntitySetMapping.OrderBy(c => c.Attribute("Name").Value));
			edmxEntityContainerMapping.Add(mappingFunctionImportMapping.OrderBy(c => c.Attribute("FunctionImportName").Value));

			File.WriteAllText(edmxPath, @"<?xml version=""1.0"" encoding=""utf-8""?>
" + edmxFile.ToString().Replace(" xmlns=\"\"", "").Replace("  ", "\t"));
		}
	}
}
