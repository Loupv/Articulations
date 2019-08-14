
class SummarySample {
  
  float f1,f2,f3,f4;
  float timeStamp;
  
  SummarySample(float f1, float f2, float f3, float f4, String time)
  {
    
    this.f1 = f1;
    this.f2 = f2;
    this.f3 = f3;
    this.f4 = f4;
    
    this.timeStamp = Float.parseFloat(time.replace(",","."));
  }
  
}
