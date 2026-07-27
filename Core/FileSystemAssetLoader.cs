using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Platform;

namespace NyxAssetsEditor.Core
{
	public class FileSystemAssetLoader : IAssetLoader
	{
		private readonly IAssetLoader _underlying;

		public FileSystemAssetLoader(IAssetLoader underlying)
		{
			_underlying = underlying;
		}

		private string? MapToPhysicalPath(Uri uri, Uri? baseUri)
		{
			var absoluteUri = uri;
			if (!uri.IsAbsoluteUri && baseUri != null)
			{
				absoluteUri = new Uri(baseUri, uri);
			}

			if (absoluteUri.Scheme != "avares") return null;

			var path = absoluteUri.LocalPath;
			if (path.StartsWith("/Assets/", StringComparison.OrdinalIgnoreCase))
			{
				var relativePath = "Assets" + path.Substring(7).Replace('/', Path.DirectorySeparatorChar);
				return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
			}

			return null;
		}

		public bool Exists(Uri uri, Uri? baseUri = null)
		{
			var physicalPath = MapToPhysicalPath(uri, baseUri);
			if (physicalPath != null)
			{
				return File.Exists(physicalPath);
			}
			return _underlying.Exists(uri, baseUri);
		}

		public Stream Open(Uri uri, Uri? baseUri = null)
		{
			var physicalPath = MapToPhysicalPath(uri, baseUri);
			if (physicalPath != null && File.Exists(physicalPath))
			{
				return File.OpenRead(physicalPath);
			}
			return _underlying.Open(uri, baseUri);
		}

		public IEnumerable<Uri> GetAssets(Uri uri, Uri? baseUri = null)
		{
			var physicalPath = MapToPhysicalPath(uri, baseUri);
			if (physicalPath != null && Directory.Exists(physicalPath))
			{
				var files = Directory.GetFiles(physicalPath, "*", SearchOption.AllDirectories);
				return files.Select(f =>
				{
					var rel = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, f)
						.Replace(Path.DirectorySeparatorChar, '/');
					return new Uri($"avares://NyxAssetsEditor/{rel}");
				});
			}
			return _underlying.GetAssets(uri, baseUri);
		}

		public System.Reflection.Assembly? GetAssembly(Uri uri, Uri? baseUri = null)
		{
			return _underlying.GetAssembly(uri, baseUri);
		}

		public void InvalidateAssemblyCache(string assemblyName)
		{
			_underlying.InvalidateAssemblyCache(assemblyName);
		}

		public void InvalidateAssemblyCache()
		{
			_underlying.InvalidateAssemblyCache();
		}

		public (Stream stream, System.Reflection.Assembly assembly) OpenAndGetAssembly(Uri uri, Uri? baseUri = null)
		{
			// Even if it maps to physical path, it should still yield the correct assembly
			var physicalPath = MapToPhysicalPath(uri, baseUri);
			if (physicalPath != null && File.Exists(physicalPath))
			{
				var stream = File.OpenRead(physicalPath);
				var assembly = GetAssembly(uri, baseUri) ?? typeof(FileSystemAssetLoader).Assembly;
				return (stream, assembly);
			}
			return _underlying.OpenAndGetAssembly(uri, baseUri);
		}

		public void SetDefaultAssembly(System.Reflection.Assembly assembly)
		{
			_underlying.SetDefaultAssembly(assembly);
		}
	}
}
