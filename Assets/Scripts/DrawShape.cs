using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawShape : MonoBehaviour
{
    public float baseContractionVolume = 10000, baseVolume = 10000, maxVolume = 0;
    public float contractionIndex;
    Mesh mesh;
    Material mat;
    public Color startCol, endCol;
    public float t ;

    void Start(){
        mesh = GetComponent<MeshFilter>().mesh;
        mat = GetComponent<Renderer>().material;
    }

    private void CreatePrism (Transform s1,Transform s2,Transform s3) {

        float minX = Mathf.Min(s1.position.x, s2.position.x, s3.position.x) - this.transform.position.x;
        float maxX = Mathf.Max(s1.position.x, s2.position.x, s3.position.x) - this.transform.position.x;
        
        float maxY = Mathf.Max(s1.position.y, s2.position.y, s3.position.y) - this.transform.position.y;
        float minY = - this.transform.position.y; 
        float minZ = Mathf.Min(s1.position.z, s2.position.z, s3.position.z) - this.transform.position.z;
        float maxZ = Mathf.Max(s1.position.z, s2.position.z, s3.position.z) - this.transform.position.z;

		Vector3[] vertices = {
			s1.position- this.transform.position,
			s2.position- this.transform.position,
            s3.position- this.transform.position,
			new Vector3 (s3.position.x, 0, s3.position.z)- this.transform.position,
			new Vector3 (s1.position.x, 0, s1.position.z)- this.transform.position,
			new Vector3 (s2.position.x, 0, s2.position.z)- this.transform.position
		};

		int[] triangles = {
			0, 1, 2, //face front
			0, 2, 3,
			0, 3, 4, //face top
			2, 1, 3,
			1, 5, 3, //face right
			1, 0, 5,
			0, 4, 5, //face left
			5, 4, 3
		};
			
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
}

    private void CreateRegularPrism (Transform s1,Transform s2,Transform s3) {
  
        float minX = Mathf.Min(s1.position.x, s2.position.x, s3.position.x) - this.gameObject.transform.position.x;
        float maxX = Mathf.Max(s1.position.x, s2.position.x, s3.position.x) - this.gameObject.transform.position.x;
        
        float maxY = Mathf.Max(s1.position.y, s2.position.y, s3.position.y);
        float minY = 0; 
        float minZ = Mathf.Min(s1.position.z, s2.position.z, s3.position.z) - this.gameObject.transform.position.z;
        float maxZ = Mathf.Max(s1.position.z, s2.position.z, s3.position.z) - this.gameObject.transform.position.z;

		Vector3[] vertices = {
			new Vector3 (s1.position.x,maxY,s1.position.z) - this.gameObject.transform.position,
			new Vector3 (s2.position.x,maxY,s2.position.z)- this.gameObject.transform.position,
            new Vector3 (s3.position.x,maxY,s3.position.z)- this.gameObject.transform.position,
			new Vector3 (s3.position.x, 0, s3.position.z)- this.gameObject.transform.position,
			new Vector3 (s1.position.x, 0, s1.position.z)- this.gameObject.transform.position,
			new Vector3 (s2.position.x, 0, s2.position.z)- this.gameObject.transform.position
		};

		int[] triangles = {
			0, 1, 2, //face front
			0, 2, 3,
			0, 3, 4, //face top
			2, 1, 3,
			1, 5, 3, //face right
			1, 0, 5,
			0, 4, 5, //face left
			5, 4, 3
		};
			
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
    }


    private void CalculateCubeVolume(){
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        float volume = (mesh.vertices[1] - mesh.vertices[0]).magnitude * (mesh.vertices[3] - mesh.vertices[0]).magnitude * (mesh.vertices[7] - mesh.vertices[0]).magnitude;
        Debug.Log(volume);
    }


    private float CalculatePrismVolume(Transform s1,Transform s2,Transform s3){
        

        //float baseVolume = 0;// base volume = aire du triangle au sol * hauteur => aire du triangle = sqr(s(s-a)(s-b)(s-c) où s est le demipérimètre (formule Héron) hauteur = min(s1.y,s2.y,s3.y)
        //float headVolume = 0; // trouver comment calculer la différence avec la partie supérieure du triangle
        float d1 = new Vector3(s1.position.x - s2.position.x,0, s1.position.z - s2.position.z).magnitude;
        float d2 = new Vector3(s2.position.x - s3.position.x,0, s2.position.z - s3.position.z).magnitude;
        float d3 = new Vector3(s3.position.x - s1.position.x,0, s3.position.z - s1.position.z).magnitude;
        
        float demiperimeter = (d1 + d2 + d3) / 2;
        float prismVolume = Mathf.Sqrt(demiperimeter*(demiperimeter-d1)*(demiperimeter-d2)*(demiperimeter-d3)) * Mathf.Max(s1.position.y, s2.position.y, s3.position.y);
        return prismVolume;
    }

    private void CreateCube (Transform s1,Transform s2,Transform s3) {

        float minX = Mathf.Min(s1.position.x, s2.position.x, s3.position.x) - this.transform.position.x;
        float maxX = Mathf.Max(s1.position.x, s2.position.x, s3.position.x) - this.transform.position.x;
        
        float maxY = Mathf.Max(s1.position.y, s2.position.y, s3.position.y) - this.transform.position.y;
        float minY = - this.transform.position.y; 
        float minZ = Mathf.Min(s1.position.z, s2.position.z, s3.position.z) - this.transform.position.z;
        float maxZ = Mathf.Max(s1.position.z, s2.position.z, s3.position.z) - this.transform.position.z;

		Vector3[] vertices = {
			new Vector3 (minX, minY, minZ),
			new Vector3 (maxX, minY, minZ),
			new Vector3 (maxX, maxY, minZ),
			new Vector3 (minX, maxY, minZ),
			new Vector3 (minX, maxY, maxZ),
			new Vector3 (maxX, maxY, maxZ),
			new Vector3 (maxX, minY, maxZ),
			new Vector3 (minX, minY, maxZ),
		};

		int[] triangles = {
			0, 2, 1, //face front
			0, 3, 2,
			2, 3, 4, //face top
			2, 4, 5,
			1, 2, 5, //face right
			1, 5, 6,
			0, 7, 4, //face left
			0, 4, 3,
			5, 4, 7, //face back
			5, 7, 6,
			0, 6, 7, //face bottom
			0, 1, 6
		};
			
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
}


    // Update is called once per frame
    public float DrawContraction(Transform s1,Transform s2,Transform s3)
    {
        //CreatePrism();
        CreateRegularPrism(s1,s2,s3);
        //CalculateCubeVolume();
        float volume = CalculatePrismVolume(s1,s2,s3);

        if(baseVolume > volume) baseVolume = volume;
        if(maxVolume < volume) maxVolume = volume;

        if(baseContractionVolume > volume) baseContractionVolume = volume;

        contractionIndex = volume/baseContractionVolume;
        t = (volume-baseVolume)/(maxVolume-baseVolume);
        mat.color = Color.Lerp(startCol, endCol, t);

        return contractionIndex;
    }
}
