
class SummarySample {
  
  float f1,f2,f3,f4,f5,f6,f7,f8,f9,f10;
  float h1;
  float timeStamp;
  
  SummarySample(float f1, float f2, float f3, float f4, float f5,float f6, float f7, float f8, float f9, float f10, String time, float h1)
  {
    
    this.f1 = f1;
    this.f2 = f2;
    this.f3 = f3;
    this.f4 = f4;
    this.f5 = f5;
    this.f6 = f6;
    this.f7 = f7;
    this.f8 = f8;
    this.f9 = f9;
    this.f10 = f10;
    this.timeStamp = Float.parseFloat(time.replace(",","."));
    this.h1 = h1;
  }
  
}
