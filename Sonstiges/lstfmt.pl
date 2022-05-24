# perl lstfmt.pl gpx-tracks.txt > z

use utf8;      # Sourcecode ist UTF8
use Encode qw(encode decode);
use strict;

binmode( STDOUT, 'utf8:' ); # Allow output of UTF8 to STDOUT

print "# GPX-Datei Name Farbe GPX-Bilddatei\n";
while(<STDIN>) {
   chomp;
   my $txt = $_;

   $txt = decode('utf-8', $txt);    # String intern als UTF8 markiert -> dann fkt. auch sprintf
   $txt =~ s/\s*$//;
   $txt =~ s/^\s*//;

   my $name = '';
   my $gpx = '';
   my $pict = '""';
   my $col = 1;

   if (length($txt) == 0) {
      
      print "\n";

   } elsif ($txt =~ /<<<.*/) {
      
      print "$txt\n";
      
   } elsif ($txt =~ /(>>>)/) {
      
      print "$txt\n";
      
   } elsif ($txt =~ /^(\"[^\"]*\")$/) {
      
      $gpx = $1;
      
   } elsif ($txt =~ /^(\"[^\"]*\")\s*(\"[^\"]*\")$/) {
      
      $gpx = $2;
      $name = $1;
      
   } elsif ($txt =~ /^(\"[^\"]*\")\s*(\"[^\"]*\")\s*(\"[^\"]*\")$/) {
      
      $gpx = $2;
      $name = $1;
      $pict = $3;
      
   } elsif ($txt =~ /^(\"[^\"]*\")\s*(\"[^\"]*\")\s*Color([\d]{1})\s*(\"[^\"]*\")$/) {
      
      # $gpx = $1;
      # $name = $2;
      # $col = $3;
      # $pict = $4;

   }

   if ($gpx =~ /\/(.)[^\/]*\"$/) {
      if ($1 eq 'W') {
         $col = 1;
      } elsif ($1 eq 'R') {
         $col = 2;
      } elsif ($1 eq 'K') {
         $col = 3;
      } else {
         if ($name =~ /wander/i) {
            $col = 1;
         } elsif ($name =~ /rad/i) {
            $col = 2;
         } elsif ($name =~ /kanu/i) {
            $col = 3;
         }
      }
   }
      
   
   if ($gpx ne '') {
      if ($pict eq '""') {
         $pict = '';
      }
      
      if ($pict eq '' &&
          $col == 1) {
         $col = 0;
      }
      
      if ($name eq '""') {
         $name = '';
      }
      
      if ($name eq '') {
         $col = 0;
         $pict = '';
      }
      
      $txt = sprintf("%-105s%-95s %s %s", 
                     $gpx, 
                     $name, 
                     $col > 0 ? "Color$col" : '', 
                     $pict);
      $txt =~ s/\s*$//;
    
      print "$txt\n";
   }
}
