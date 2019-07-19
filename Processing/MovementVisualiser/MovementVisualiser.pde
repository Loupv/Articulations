import oscP5.*;
import netP5.*;
import java.util.Collections;
import java.util.Comparator;

OscP5 oscP5;
OscMessage myMessage;
OscP5 multicastOsc;

ArrayList<SummarySample> dataList;
ArrayList<Integer> dataId;

MyLock myLock;

float plottedMs = 12000.0;

void setup() {
  size(1200, 1000);
  frameRate(30);


  // The following is needed for Macs to get the Multicast
  System.setProperty("java.net.preferIPv4Stack", "true");

  myLock = new MyLock();
  dataList = new ArrayList<SummarySample>();
  dataId = new ArrayList<Integer>();

  oscP5 = new OscP5(this, 7979);
  //initializeReceiving();
}

void draw() {
  background(0);

  strokeWeight(2);

  colorMode(HSB, 100);

  int tSize = 32;
  
  // dancer 1
  textSize(tSize);
  fill(85, 100, 100);
  text("p1speedLeft", 10, (height * 0.05) + tSize);
  fill(50, 100, 100);
  text("p1accLeft", 10, (height * 0) + tSize);

  // dancer 2
  fill(100, 100, 100);
  text("p2speedLeft", 10 +  width/2, (height * 0.05) + tSize);
  fill(60, 100, 100);
  text("p2accLeft", 10 +  width/2, (height * 0) + tSize);


 
  

  // Finding the max timestamp
  
  float maxTime = dataList.get(dataList.size()-1).timeStamp;
  
    
  /*for (int i = dataList.size()-1; i >= 0; i--)
  {
    if (dataList.get(i).timeStamp < maxTime - 10000 ) // remove if data is 10s old
    {
      for(int j = dataList.size() - i; j >0; j--)
        dataList.remove(j);
    }
  }
*/
  
  // Plotting the data

  // To change the colors you should define here a variable that depends on the listInd. Basically, listInd 0 means data from one harness, 
    // and listInd 1 is from another. 
    // If you then give your variable as the first value to the stroke methods below, you will change the hues of the graphs.

    int colorGap = 0;
    

   if (dataList.size() > 1)
    {
      //stroke((15*listInd) % 100, 100, 100);

      float xStart = maxTime - (maxTime % 10000) + 10000;

      for (int i = 1; i < dataList.size()-1; i++)
      {

        if (dataList.get(i).timeStamp < maxTime - 10000 ) dataList.remove(i);
        else{       
          SummarySample sum0 = dataList.get(i-1);
          SummarySample sum1 = dataList.get(i);
          
          if (sum1.timeStamp - sum0.timeStamp < 2000)
          {
            
            
            // speed
            stroke(50 + colorGap, 100, 100);     
            line(width - ((xStart - sum0.timeStamp) / plottedMs * width), 
              (sum0.f1 * -3000.0) + (height * 0.49), 
              width - ((xStart - sum1.timeStamp)  / plottedMs * width), 
              (sum1.f1 * -3000.0) + (height * 0.49));
              
              // acc
            stroke(85 + 2*colorGap, 100, 100);      
            line(width - ((xStart - sum0.timeStamp) / plottedMs * width), 
              (sum0.f2 * -10000.0) + (height * 0.99), 
              width - ((xStart - sum1.timeStamp) / plottedMs * width), 
              (sum1.f2 * -10000.0) + (height * 0.99));
            
          }
        }
      }
  }

}



/* incoming osc message are forwarded to the oscEvent method. */
void oscEvent(OscMessage message)
{
  if (message.checkAddrPattern("/Test") == true)
  {
    int playerID = message.get(0).intValue();
    int handedness = message.get(1).intValue();
    
    float f1 = message.get(2).floatValue();
    float f2 = message.get(3).floatValue();
    float f3 = message.get(4).floatValue();
    float f4 = message.get(5).floatValue();
    float f5 = message.get(6).floatValue();
    float timeStamp = message.get(7).floatValue();
    
   
    //SummarySample data = new SummarySample();
    dataList.add(new SummarySample(playerID, handedness, f1,f2,f3,f4,f5, timeStamp));
    //dataList.add(data);
    println(dataList.size());
    
  }

   
  
}
