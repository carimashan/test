using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitware.VTK;

namespace Sirius.CTA.Segment
{
    public class Heart
    {   
        public itk.simple.Image itkimage=null;
        uint[] dim=new uint[3];
        public vtkImageData vtkimage = null;
        public vtkImageData maskArtery = null;
        public vtkImageData maskBone=null;
        public vtkImageData maskMuscle= null;
        public float[] origin = null;
        public float[] space= null;

        public vtkPolyData maskArteryMesh=null;
        public Heart(itk.simple.Image itkimage) 
        { 
            this.itkimage=itk.simple.SimpleITK.RescaleIntensity(itkimage,0,1);
            this.itkimage = itk.simple.SimpleITK.Cast(this.itkimage, itk.simple.PixelIDValueEnum.sitkFloat32);
            
            dim = itkimage.GetSize().ToArray();
            var simg = itk.simple.SimpleITK.RescaleIntensity(this.itkimage, 0, 255);
            simg = itk.simple.SimpleITK.Cast(simg, itk.simple.PixelIDValueEnum.sitkUInt8);
            vtkimage = Sirius.Utilis.ImageConverter.itk2vtkimage(simg);
            float[] space = Program.projectData.imageInfo.space;
            vtkimage.SetSpacing(space[0], space[1], space[2]);

            Program.projectData.ctaProjectData.vtkImage = vtkimage;
            
            search();
        }
        public Heart(itk.simple.Image[] itkimages)
        {
            this.itkimage = itk.simple.SimpleITK.RescaleIntensity(itkimage, 0, 1);
            this.itkimage = itk.simple.SimpleITK.Cast(this.itkimage, itk.simple.PixelIDValueEnum.sitkFloat32);

            dim = itkimage.GetSize().ToArray();
            var simg = itk.simple.SimpleITK.RescaleIntensity(this.itkimage, 0, 255);
            simg = itk.simple.SimpleITK.Cast(simg, itk.simple.PixelIDValueEnum.sitkUInt8);
            vtkimage = Sirius.Utilis.ImageConverter.itk2vtkimage(simg);
            float[] space = Program.projectData.imageInfo.space;
            vtkimage.SetSpacing(space[0], space[1], space[2]);
            search();
        }

        private void search()
        {
            itk.simple.VectorUInt32 vec=new itk.simple.VectorUInt32(new uint[] { dim[0] / 2, dim[1] / 2, dim[2]/2 });
            itk.simple.VectorUIntList veclist = new itk.simple.VectorUIntList();
            veclist.Add(vec);
            var centrevalue=itkimage.GetPixelAsFloat(vec)*0.95f;
            var mask = itk.simple.SimpleITK.ConnectedThreshold(itkimage, veclist, centrevalue);
            mask = itk.simple.SimpleITK.RescaleIntensity(mask, 0, 255);
            mask = itk.simple.SimpleITK.Cast(mask, itk.simple.PixelIDValueEnum.sitkUInt8);
            maskArtery = Sirius.Utilis.ImageConverter.itk2vtkimage(mask);

            maskArteryMesh = getmesh(maskArtery);
            Program.projectData.ctaProjectData.LVcavityMaskPolydata= maskArteryMesh;
        }

        private vtkPolyData getmesh(vtkImageData binaryImage)         
        {
            vtkMarchingContourFilter m = vtkMarchingContourFilter.New();
            m.SetInput(binaryImage);
            m.SetNumberOfContours(1);
            m.SetValue(0, 1);
            m.SetComputeNormals(0);
            m.SetComputeGradients(0);
            m.Update();

            return m.GetOutput();
        }

    }
}
