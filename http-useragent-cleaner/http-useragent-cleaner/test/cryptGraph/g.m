close all

hold on
format('longG');

if (1 < 2)

y = [56, 80, 112, 128, 192, 256];
for i=1:length(y)
    f = @(x) getK(2015+x, 0.9, y(i));
    fplot(f, [0 200 0.0 1.0]);
end

else

y  = [80, 102, 224/2, 124, 128, 135, 142, 192, 194, 200, 256, 265];
r = [];

for i=1:length(y)
    a = getA(y(i), 2015, 0.0);
    f = @(x) getK(2015+x, a, y(i));
    fplot(f, [0 150 0.0 1.0]);
    
    r(length(r) + 1) = y(i);
    r(length(r) + 1) = a;
end

r
end

grid minor
hold off