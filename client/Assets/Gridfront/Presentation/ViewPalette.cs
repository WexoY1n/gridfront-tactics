using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gridfront.Client.Presentation
{
    /// <summary>
    /// Shared runtime materials so primitive tiles do not clone a material per renderer.
    /// </summary>
    public sealed class ViewPalette
    {
        private readonly List<Material> _owned = new List<Material>();
        private readonly Shader _lit;
        private readonly Shader _unlit;

        public ViewPalette()
        {
            _lit = Shader.Find("Standard");
            if (_lit == null)
            {
                throw new InvalidOperationException("Standard shader is missing; cannot tint demo primitives.");
            }

            _unlit = Shader.Find("Sprites/Default");
            if (_unlit == null)
            {
                throw new InvalidOperationException("Sprites/Default shader is missing; cannot draw overlays.");
            }
        }

        public Material Lit(Color color)
        {
            var material = new Material(_lit)
            {
                color = color,
                enableInstancing = true
            };
            _owned.Add(material);
            return material;
        }

        public Material Unlit(Color color)
        {
            var material = new Material(_unlit)
            {
                color = color
            };
            _owned.Add(material);
            return material;
        }

        public void Dispose()
        {
            for (var i = 0; i < _owned.Count; i++)
            {
                if (_owned[i] != null)
                {
                    UnityEngine.Object.Destroy(_owned[i]);
                }
            }

            _owned.Clear();
        }

        public static GameObject Primitive(
            PrimitiveType type,
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.Destroy(collider);
            }

            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }
    }
}
