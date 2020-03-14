function [a] = getA(bits, year, chance)

y = (bits-56)*1.5+year;
%y = year;

r = 5;
a = 0.95;
maxa = 1.0;
mina = 0.0;

prea = 0;

if (y <= 0)
    a = 1.0;
    return;
end

while (abs(r - chance) > 1e-7)
    r = getK(y, a, 56); % вероятность стойкости шифра

    if (abs(r - chance) < 1e-7)
        break;
    end

    if (r - chance > 0)
        maxa = a;
    else
        mina = a;
    end

    prea = a;
    a = (maxa+mina)/2;

    if (prea == a)
        break;
    end
end

