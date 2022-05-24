# perl xmlfmt.pl gpx-tracks.txt > gpx-tracks.gpxl

use utf8;      # Sourcecode ist UTF8
use Encode qw(encode decode);
use strict;

binmode( STDOUT, 'utf8:' ); # Allow output of UTF8 to STDOUT



# "!Sammlung/W 20200801, Falkenau, and der Flöha, 25,0km.gpx"                        "Wanderung 1.8.2020, Falkenau, and der Flöha, 25,0km"              Color1 "!Sammlung/Bilder20200801.gpx"
# 
# <<< Frauenstein 7/2020
# "202007 Frauenstein/W 20200726, Frauenstein - Muldenhütten, 23,6km.gpx"            "Wanderung 26.7.2020, Frauenstein - Muldenhütten, 23,6km"          Color1 "202007 Frauenstein/Bilder20200726.gpx"
# "202007 Frauenstein/W 20200725, Klingenberg-Colmnnitz - Frauenstein, 23,0km.gpx"   "Wanderung 25.7.2020, Klingenberg-Colmnnitz - Frauenstein, 23,0km" Color1 "202007 Frauenstein/Bilder20200725.gpx"
# >>>
# 
# "!Sammlung/R 20200719, Wurzen - Dahlener Heide, 62,7km+7km.gpx"                    "Radtour 19.7.2020, Wurzen - Dahlener Heide, 62,7km+7km"           Color2 "!Sammlung/Bilder20200719.gpx"




#print "# GPX-Datei (rel. bzgl. Arbeitsverzeichnis) Name Farbe GPX-Bilddatei (rel. bzgl. Arbeitsverzeichnis)\n";

print "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
print "<gpxlist>\n";

my $group = '';
my $groupcclosed = 1;
my $printgroupcclosed = 1;
while(<STDIN>) {
   chomp;
   my $txt = $_;

   $txt = decode('utf-8', $txt);    # String intern als UTF8 markiert -> dann fkt. auch sprintf

   my $name = '';
   my $gpx = '';
   my $pict = '';
   my $col = 1;
   
   if ($txt =~ s/\s*(\"[^\"]*\")\s*\"([^\"]*)\"//) {
      $gpx = $1;
      $name = $2;
      if ($txt =~ s/\s*(\d+)//) {
         $col = $1;
         if ($txt =~ /\s*(\"[^\"]*\")/) {
            $pict = $1;
         }
      }
      
      if ($groupcclosed) {
         if (!$printgroupcclosed) {
            print "   </group>\n";
            $printgroupcclosed = 1;
         }
         print "   <group name=\"$group\">\n";
         $groupcclosed = 0;
         $printgroupcclosed = 0;
      }
      
      print "      <gpxfile name=\"$name\"\n";
      print "               gpxfile=$gpx";
      if ($pict ne '') {
         print "\n               picturegpxfile=$pict";
      }
      if ($col != 1) {
         print "\n               trackcolorno=\"$col\"";
      }
      print "/>\n";
      
   } elsif ($txt =~ /^\s*<<<\s*(.+)\s*$/) {
      $group = $1;
      $groupcclosed = 1;
      #print STDERR "$group\n";
   } elsif ($txt =~ /^\s*>>>/) {
      $group = '';
      $groupcclosed = 1;
      $printgroupcclosed = 1;
      print "   </group>\n";
   }
}
if (!$groupcclosed) {
   print "   </group>\n";
}
print "</gpxlist>\n";
