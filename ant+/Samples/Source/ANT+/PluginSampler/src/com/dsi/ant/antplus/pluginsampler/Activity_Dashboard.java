/*
This software is subject to the license described in the License.txt file 
included with this software distribution. You may not use this file except in compliance 
with this license.

Copyright (c) Dynastream Innovations Inc. 2013
All rights reserved.
*/

package com.dsi.ant.antplus.pluginsampler;

import android.app.ListActivity;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.ListView;
import android.widget.SimpleAdapter;
import android.widget.Toast;

import com.dsi.ant.antplus.pluginsampler.geocache.Activity_GeoScanList;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * Dashboard 'menu' of available sampler activities
 */
public class Activity_Dashboard extends ListActivity
{
    //Initialize the list
    @SuppressWarnings("serial") //Suppress warnings about hash maps not having custom UIDs
    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_dashboard);     
        List<Map<String,String>> menuItems = new ArrayList<Map<String,String>>();
        menuItems.add(new HashMap<String,String>(){{put("title","Heart Rate Display");put("desc","Receive from HRM sensors");}});
        menuItems.add(new HashMap<String,String>(){{put("title","Stride SDM Display");put("desc","Receive from SDM sensors");}});
        menuItems.add(new HashMap<String,String>(){{put("title","Environment Display");put("desc","Receive from Tempe sensors");}});
        menuItems.add(new HashMap<String,String>(){{put("title","Geocache Utility");put("desc","Read and program Geocache sensors");}});
        menuItems.add(new HashMap<String,String>(){{put("title","Audio Controllable Device");put("desc","Transmit audio player status and receive commands from remote control");}});
        menuItems.add(new HashMap<String,String>(){{put("title","Video Controllable Device");put("desc","Transmit video player status and receive commands from remote control");}});
        menuItems.add(new HashMap<String,String>(){{put("title","Generic Controllable Device");put("desc","Receive generic commands from remote control");}});
        menuItems.add(new HashMap<String,String>(){{put("title","Weight Scale Display");put("desc","Receive from weight scales");}});
        
        SimpleAdapter adapter = new SimpleAdapter(this, menuItems, android.R.layout.simple_list_item_2, new String[]{"title","desc"}, new int[]{android.R.id.text1,android.R.id.text2});
        setListAdapter(adapter);
    }

    //Launch the appropriate activity/action when a selection is made
    @Override
    protected void onListItemClick(ListView l, View v, int position, long id)
    {
        int j=0;
        
        if(position == j++)
        {
            Intent i = new Intent(this, Activity_HeartRateSampler.class);
            startActivity(i);
        }
        else if(position == j++)
        {
    	    Intent i = new Intent(this, Activity_StrideSdmSampler.class);
            startActivity(i);
    	}
        else if(position == j++)
        {
            Intent i = new Intent(this, Activity_EnvironmentSampler.class);
            startActivity(i);
        }
        else if(position == j++)
        {
            Intent i = new Intent(this, Activity_GeoScanList.class);
            startActivity(i);
        }
        else if(position == j++)
        {
            Intent i = new Intent(this, Activity_AudioControllableDeviceSampler.class);
            startActivity(i);
        }
        else if(position == j++)
        {
            Intent i = new Intent(this, Activity_VideoControllableDeviceSampler.class);
            startActivity(i);
        }
        else if(position == j++)
        {
            Intent i = new Intent(this, Activity_GenericControllableDeviceSampler.class);
            startActivity(i);
        }
        else if(position == j++)
        {
            Intent i = new Intent(this, Activity_WeightScaleSampler.class);
            startActivity(i);
        }
        else
        {
            Toast.makeText(this, "This menu item is not implemented", Toast.LENGTH_SHORT).show();  
        }
    }
    
    

}
