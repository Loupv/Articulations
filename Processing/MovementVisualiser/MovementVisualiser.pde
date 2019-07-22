import oscP5.*;
import netP5.*;
import java.util.Collections;
import java.util.Comparator;

OscP5 oscP5;
OscMessage myMessage;
OscP5 multicastOsc;

ArrayList<SummarySample> dataListP1, dataListP2;

MyLock myLock;

float plottedMs = 10000.0;

void setup() {
  size(1200, 1000);
  frameRate(30);


  // The following is needed for Macs to get the Multicast
  System.setProperty("java.net.preferIPv4Stack", "true");

  myLock = new MyLock();
  dataListP1 = new ArrayList<SummarySample>();
  dataListP2 = new ArrayList<SummarySample>();

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
  //float timeFrame = 10000;




  if (dataListP1.size() > 1)
  {
    //stroke((15*listInd) % 100, 100, 100);
    float lastTimeStamp = dataListP1.get(dataListP1.size()-1).timeStamp;

    for (int i = 1; i < dataListP1.size()-1; i++)
    {
      if (dataListP1.get(i).timeStamp < lastTimeStamp - plottedMs ) dataListP1.remove(i);
    }

    float xStart = lastTimeStamp - (lastTimeStamp % 10000)  + 10000;

    for (int i = 1; i < dataListP1.size(); i++)
    {

      SummarySample sum0 = dataListP1.get(i-1);
      SummarySample sum1 = dataListP1.get(i);

      if (sum1.timeStamp - sum0.timeStamp < 2000)
      {    
        //println(sum0.timeStamp+", "+sum0.f1);

        // speed Left
        stroke(50 + colorGap, 100, 100);     
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2 +width/2), 
          (sum0.f1 * -1000.0) + (height * 0.25), 
          width - ((xStart - sum1.timeStamp)  / plottedMs * width/2 +width/2), 
          (sum1.f1 * -1000.0) + (height * 0.25));

        // acc Left
        stroke(85 + 2*colorGap, 100, 100);      
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2 +width/2), 
          (sum0.f2 * -10000.0) + (height * 0.50), 
          width - ((xStart - sum1.timeStamp) / plottedMs * width/2 +width/2), 
          (sum1.f2 * -10000.0) + (height * 0.50));

        // speed Right
        stroke(50 + colorGap, 100, 100);     
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2 +width/2), 
          (sum0.f6 * -1000.0) + (height * 0.75), 
          width - ((xStart - sum1.timeStamp)  / plottedMs * width/2 +width/2), 
          (sum1.f6 * -1000.0) + (height * 0.75));

        // acc Right
        stroke(85 + 2*colorGap, 100, 100);      
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2 +width/2), 
          (sum0.f7 * -10000.0) + (height * 0.99), 
          width - ((xStart - sum1.timeStamp) / plottedMs * width/2 +width/2), 
          (sum1.f7 * -10000.0) + (height * 0.99));
      }
    }
  }





  if (dataListP2.size() > 1)
  {

    float lastTimeStamp = dataListP2.get(dataListP2.size()-1).timeStamp;
  float firstTimeStamp = dataListP2.get(0).timeStamp;

    for (int i = 1; i < dataListP2.size(); i++)
    {
      if (dataListP2.get(i).timeStamp < lastTimeStamp - plottedMs ) dataListP2.remove(i);
    }

    // dernier temps arrondi à la seconde + nombre de secondes à afficher 
    float xStart = lastTimeStamp - (lastTimeStamp % 10000) + plottedMs;

    println(dataListP2.size());
    for (int i = 1; i < dataListP2.size(); i++)
    {
      SummarySample sum0 = dataListP2.get(i-1);
      SummarySample sum1 = dataListP2.get(i);
      if (sum1.timeStamp - sum0.timeStamp < 2000)
      {   

        // (lastTimeStamp - (lastTimeStamp % 10000) + plottedM - sum0.timestamp) / plottedMs
        // (lastTimeStamp - (lastTimeStamp % 10000) - sum0.timestamp) / plottedMs + 1
        
        // temps max en sec - temps actuel / plottedMs
        
        // speed Left
        stroke(50 + colorGap+20, 100, 100);     
        line(width - ((lastTimeStamp - sum0.timeStamp - firstTimeStamp) / (lastTimeStamp-firstTimeStamp) * width/2), 
          (sum0.f1 * -1000.0) + (height * 0.25), 
          width - ((lastTimeStamp - sum0.timeStamp - firstTimeStamp) / (lastTimeStamp-firstTimeStamp) * width/2), 
          (sum1.f1 * -1000.0) + (height * 0.25));

        // acc Left
        stroke(85 + 2*colorGap, 100, 100);      
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2), 
          (sum0.f2 * -10000.0) + (height * 0.49), 
          width - ((xStart - sum1.timeStamp) / plottedMs * width/2), 
          (sum1.f2 * -10000.0) + (height * 0.49));

        // contraction
        /*stroke(85 + 2*colorGap, 100, 100);      
         line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2), 
         (sum0.h1 * -10000.0) + (height * 0.49), 
         width - ((xStart - sum1.timeStamp) / plottedMs * width/2), 
         (sum1.h1 * -10000.0) + (height * 0.49));
         */

        // speed Right
        stroke(50 + colorGap, 100, 100);     
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2), 
          (sum0.f6 * -1000.0) + (height * 0.75), 
          width - ((xStart - sum1.timeStamp)  / plottedMs * width/2), 
          (sum1.f6 * -1000.0) + (height * 0.75));

        // acc Right
        stroke(85 + 2*colorGap, 100, 100);      
        line(width - ((xStart - sum0.timeStamp) / plottedMs * width/2), 
          (sum0.f7 * -10000.0) + (height * 0.99), 
          width - ((xStart - sum1.timeStamp) / plottedMs * width/2), 
          (sum1.f7 * -10000.0) + (height * 0.99));
      }
    }
  }
}



/* incoming osc message are forwarded to the oscEvent method. */
void oscEvent(OscMessage message)
{
  if (message.checkAddrPattern("/P1") == true)
  {
    float f1 = message.get(0).floatValue();
    float f2 = message.get(1).floatValue();
    float f3 = message.get(2).floatValue();
    float f4 = message.get(3).floatValue();
    float f5 = message.get(4).floatValue();
    float f6 = message.get(5).floatValue();
    float f7 = message.get(6).floatValue();
    float f8 = message.get(7).floatValue();
    float f9 = message.get(8).floatValue();
    float f10 = message.get(9).floatValue();
    String timeStamp = message.get(10).stringValue();
    float h1 = 0;//message.get(11).floatValue();

    dataListP1.add(new SummarySample(f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, timeStamp, h1));
  } else if (message.checkAddrPattern("/P2") == true)
  {

    float f1 = message.get(0).floatValue();
    float f2 = message.get(1).floatValue();
    float f3 = message.get(2).floatValue();
    float f4 = message.get(3).floatValue();
    float f5 = message.get(4).floatValue();
    float f6 = message.get(5).floatValue();
    float f7 = message.get(6).floatValue();
    float f8 = message.get(7).floatValue();
    float f9 = message.get(8).floatValue();
    float f10 = message.get(9).floatValue();
    String timeStamp = message.get(10).stringValue();
    float h1 = 0;//message.get(11).floatValue();


    dataListP2.add(new SummarySample(f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, timeStamp, h1));
  }
}
