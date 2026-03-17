/*
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
var showControllersOnly = false;
var seriesFilter = "";
var filtersOnlySampleSeries = true;

/*
 * Add header in statistics table to group metrics by category
 * format
 *
 */
function summaryTableHeader(header) {
    var newRow = header.insertRow(-1);
    newRow.className = "tablesorter-no-sort";
    var cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 1;
    cell.innerHTML = "Requests";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 3;
    cell.innerHTML = "Executions";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 7;
    cell.innerHTML = "Response Times (ms)";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 1;
    cell.innerHTML = "Throughput";
    newRow.appendChild(cell);

    cell = document.createElement('th');
    cell.setAttribute("data-sorter", false);
    cell.colSpan = 2;
    cell.innerHTML = "Network (KB/sec)";
    newRow.appendChild(cell);
}

/*
 * Populates the table identified by id parameter with the specified data and
 * format
 *
 */
function createTable(table, info, formatter, defaultSorts, seriesIndex, headerCreator) {
    var tableRef = table[0];

    // Create header and populate it with data.titles array
    var header = tableRef.createTHead();

    // Call callback is available
    if(headerCreator) {
        headerCreator(header);
    }

    var newRow = header.insertRow(-1);
    for (var index = 0; index < info.titles.length; index++) {
        var cell = document.createElement('th');
        cell.innerHTML = info.titles[index];
        newRow.appendChild(cell);
    }

    var tBody;

    // Create overall body if defined
    if(info.overall){
        tBody = document.createElement('tbody');
        tBody.className = "tablesorter-no-sort";
        tableRef.appendChild(tBody);
        var newRow = tBody.insertRow(-1);
        var data = info.overall.data;
        for(var index=0;index < data.length; index++){
            var cell = newRow.insertCell(-1);
            cell.innerHTML = formatter ? formatter(index, data[index]): data[index];
        }
    }

    // Create regular body
    tBody = document.createElement('tbody');
    tableRef.appendChild(tBody);

    var regexp;
    if(seriesFilter) {
        regexp = new RegExp(seriesFilter, 'i');
    }
    // Populate body with data.items array
    for(var index=0; index < info.items.length; index++){
        var item = info.items[index];
        if((!regexp || filtersOnlySampleSeries && !info.supportsControllersDiscrimination || regexp.test(item.data[seriesIndex]))
                &&
                (!showControllersOnly || !info.supportsControllersDiscrimination || item.isController)){
            if(item.data.length > 0) {
                var newRow = tBody.insertRow(-1);
                for(var col=0; col < item.data.length; col++){
                    var cell = newRow.insertCell(-1);
                    cell.innerHTML = formatter ? formatter(col, item.data[col]) : item.data[col];
                }
            }
        }
    }

    // Add support of columns sort
    table.tablesorter({sortList : defaultSorts});
}

$(document).ready(function() {

    // Customize table sorter default options
    $.extend( $.tablesorter.defaults, {
        theme: 'blue',
        cssInfoBlock: "tablesorter-no-sort",
        widthFixed: true,
        widgets: ['zebra']
    });

    var data = {"OkPercent": 64.57250826641474, "KoPercent": 35.42749173358526};
    var dataset = [
        {
            "label" : "FAIL",
            "data" : data.KoPercent,
            "color" : "#FF6347"
        },
        {
            "label" : "PASS",
            "data" : data.OkPercent,
            "color" : "#9ACD32"
        }];
    $.plot($("#flot-requests-summary"), dataset, {
        series : {
            pie : {
                show : true,
                radius : 1,
                label : {
                    show : true,
                    radius : 3 / 4,
                    formatter : function(label, series) {
                        return '<div style="font-size:8pt;text-align:center;padding:2px;color:white;">'
                            + label
                            + '<br/>'
                            + Math.round10(series.percent, -2)
                            + '%</div>';
                    },
                    background : {
                        opacity : 0.5,
                        color : '#000'
                    }
                }
            }
        },
        legend : {
            show : true
        }
    });

    // Creates APDEX table
    createTable($("#apdexTable"), {"supportsControllersDiscrimination": true, "overall": {"data": [0.0, 500, 1500, "Total"], "isController": false}, "titles": ["Apdex", "T (Toleration threshold)", "F (Frustration threshold)", "Label"], "items": [{"data": [0.0, 500, 1500, "GET /api/Reports/summary (sayfa 2)"], "isController": false}, {"data": [0.0, 500, 1500, "GET /api/Reports/summary (sayfa 1)"], "isController": false}, {"data": [0.0, 500, 1500, "GET /api/Reports/export (xlsx)"], "isController": false}, {"data": [0.0, 500, 1500, "POST /api/Reports/queue"], "isController": false}, {"data": [0.0, 500, 1500, "GET /api/Movies/1 (tekil)"], "isController": false}, {"data": [0.0, 500, 1500, "GET /api/Reports/export (csv)"], "isController": false}, {"data": [0.0, 500, 1500, "GET /api/Movies (liste)"], "isController": false}]}, function(index, item){
        switch(index){
            case 0:
                item = item.toFixed(3);
                break;
            case 1:
            case 2:
                item = formatDuration(item);
                break;
        }
        return item;
    }, [[0, 0]], 3);

    // Create statistics table
    createTable($("#statisticsTable"), {"supportsControllersDiscrimination": true, "overall": {"data": ["Total", 2117, 750, 35.42749173358526, 47149.20217288614, 2415, 127194, 14248.0, 113685.4, 122454.9, 125986.36000000002, 15.47480683903131, 52.411945119076336, 1.6092726059552789], "isController": false}, "titles": ["Label", "#Samples", "FAIL", "Error %", "Average", "Min", "Max", "Median", "90th pct", "95th pct", "99th pct", "Transactions/s", "Received", "Sent"], "items": [{"data": ["GET /api/Reports/summary (sayfa 2)", 289, 101, 34.94809688581315, 47001.38754325257, 9281, 115205, 14888.0, 113650.0, 114554.0, 114884.0, 2.198805493209571, 9.678864850020922, 0.21930398105527446], "isController": false}, {"data": ["GET /api/Reports/summary (sayfa 1)", 438, 149, 34.018264840182646, 43715.401826484005, 4496, 115101, 12972.5, 109266.3, 113143.34999999999, 114969.97, 3.201988449448059, 13.893666557771036, 0.32186083595292053], "isController": false}, {"data": ["GET /api/Reports/export (xlsx)", 251, 83, 33.06772908366534, 44096.61752988047, 7625, 114688, 13593.0, 113070.0, 113719.59999999999, 114409.92, 1.8904730701734567, 10.380103771248994, 0.19523747655738077], "isController": false}, {"data": ["POST /api/Reports/queue", 250, 0, 0.0, 8193.759999999998, 2415, 12409, 8505.0, 11268.3, 11744.8, 12217.880000000001, 14.742304517042104, 12.485752944775328, 3.2824662401226563], "isController": false}, {"data": ["GET /api/Movies/1 (tekil)", 201, 181, 90.04975124378109, 102234.84577114429, 25050, 123796, 108559.0, 119660.8, 123357.2, 123729.78, 1.5500528251833459, 4.531666311606888, 0.019279263994817733], "isController": false}, {"data": ["GET /api/Reports/export (csv)", 368, 117, 31.793478260869566, 41043.03260869566, 2635, 113776, 12387.5, 108240.4, 110242.1, 113760.79, 2.6902551356093283, 6.394738856093282, 0.2813319572885445], "isController": false}, {"data": ["GET /api/Movies (liste)", 320, 119, 37.1875, 57232.446875000016, 6537, 127194, 23820.0, 125439.8, 126216.85, 127022.79000000001, 2.339266786066742, 6.909417947476151, 0.1807991657224314], "isController": false}]}, function(index, item){
        switch(index){
            // Errors pct
            case 3:
                item = item.toFixed(2) + '%';
                break;
            // Mean
            case 4:
            // Mean
            case 7:
            // Median
            case 8:
            // Percentile 1
            case 9:
            // Percentile 2
            case 10:
            // Percentile 3
            case 11:
            // Throughput
            case 12:
            // Kbytes/s
            case 13:
            // Sent Kbytes/s
                item = item.toFixed(2);
                break;
        }
        return item;
    }, [[0, 0]], 0, summaryTableHeader);

    // Create error table
    createTable($("#errorsTable"), {"supportsControllersDiscrimination": false, "titles": ["Type of error", "Number of errors", "% in errors", "% in all samples"], "items": [{"data": ["Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 750, 100.0, 35.42749173358526], "isController": false}]}, function(index, item){
        switch(index){
            case 2:
            case 3:
                item = item.toFixed(2) + '%';
                break;
        }
        return item;
    }, [[1, 1]]);

        // Create top5 errors by sampler
    createTable($("#top5ErrorsBySamplerTable"), {"supportsControllersDiscrimination": false, "overall": {"data": ["Total", 2117, 750, "Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 750, "", "", "", "", "", "", "", ""], "isController": false}, "titles": ["Sample", "#Samples", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors", "Error", "#Errors"], "items": [{"data": ["GET /api/Reports/summary (sayfa 2)", 289, 101, "Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 101, "", "", "", "", "", "", "", ""], "isController": false}, {"data": ["GET /api/Reports/summary (sayfa 1)", 438, 149, "Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 149, "", "", "", "", "", "", "", ""], "isController": false}, {"data": ["GET /api/Reports/export (xlsx)", 251, 83, "Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 83, "", "", "", "", "", "", "", ""], "isController": false}, {"data": [], "isController": false}, {"data": ["GET /api/Movies/1 (tekil)", 201, 181, "Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 181, "", "", "", "", "", "", "", ""], "isController": false}, {"data": ["GET /api/Reports/export (csv)", 368, 117, "Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 117, "", "", "", "", "", "", "", ""], "isController": false}, {"data": ["GET /api/Movies (liste)", 320, 119, "Non HTTP response code: java.net.SocketException/Non HTTP response message: Connection reset", 119, "", "", "", "", "", "", "", ""], "isController": false}]}, function(index, item){
        return item;
    }, [[0, 0]], 0);

});
